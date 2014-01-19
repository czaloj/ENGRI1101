using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ZLibrary.Graphs;
using ZLibrary.Algorithms;
using XNA2D.Cameras;
using BlisterUI;
using BlisterUI.Input;
using ORLabs.Graphics;
using ORLabs.Graphics.Graphs;
using ORLabs.Graphics.Widgets;
using ORLabs.Framework;
using ORLabs.Algorithms;
using ZLibrary.Math;
using System.Threading;

namespace ORLabs.Screens {
    public abstract class LabGraphSreen<A, I, O> : GraphScreen
        where A : TSAOR<I, O>
        where I : struct
        where O : struct {
        public const float NEDataWidth = 240, NEDataHeight = 200, NETextOffset = 10;
        public const float SpeedWidgetHeight = 40, ActionTextHeight = SpeedWidgetHeight + 26;
        public const float ActionTextXOffset = 4;
        public const float AAWWidth = 600, AAWHeight = 500;

        protected abstract string LabName { get; }
        protected abstract string AvailableActionText { get; }
        protected const float ListMoveDuration = 0.2f;

        public bool[] KeyFlags = new bool[KeyFlagCount];
        public const int KeyFlagCount = 1;
        public const int KF_BeginAlgorithm = 0;

        protected A alg;
        protected bool resultTaken;

        protected Texture2D pixel;
        protected SpriteFont font;
        protected SpriteFont dataFont;
        const int MCC = 150;
        protected Frame dataFrame;
        protected GraphDataWidget gData;
        protected MTransVisibleList neData;
        protected SimpleText actionText;

        public override void build() {
            font = game.Content.Load<SpriteFont>(GUI.SmallFontFile);
            dataFont = game.Content.Load<SpriteFont>(GUI.MediumFontFile);

            // White Pixel Texture
            pixel = new Texture2D(game.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            #region Node/Edge Data
            gData = new GraphDataWidget(font);
            gData.FontColor = GUI.Colors.WidgetText;

            dataFrame = new Frame();
            Frame.create<Frame>(ref dataFrame, WidgetFrame.Identity, new Vector2(NEDataWidth, NEDataHeight), GUI.Colors.WidgetBackground);

            neData = new MTransVisibleList(
                new MTVLBinding(dataFrame, WidgetFrame.Identity),
                new MTVLBinding(gData, new WidgetFrame(new Vector2(NETextOffset), 0))
                );
            neData.World = new WidgetFrame(new Vector2(game.GraphicsDevice.Viewport.Width - NEDataWidth, 0), 0.5f);
            #endregion

            buildGraph();
            buildRenderer();

            buildASW();
            buildAAW();
            actionText = new SimpleText(dataFont);
            actionText.World = new WidgetFrame(new Vector2(ActionTextXOffset, game.GraphicsDevice.Viewport.Height - ActionTextHeight), 0);
            actionText.setVisible(false);
            actionText.setText("");
            actionText.Color = GUI.Colors.WidgetText;

            ORGraph.Node.OnNewHoverNode += updateInfo;
            ORGraph.Edge.OnNewHoverEdge += updateInfo;

            game.Window.ClientSizeChanged += (o, s) => {
                neData.World = new WidgetFrame(new Vector2(game.GraphicsDevice.Viewport.Width - NEDataWidth, 0), 0.5f);
                asw.World = new WidgetFrame(new Vector2(0, game.GraphicsDevice.Viewport.Height - SpeedWidgetHeight), 0);
                actionText.World = new WidgetFrame(new Vector2(ActionTextXOffset, game.GraphicsDevice.Viewport.Height - ActionTextHeight), 0);
            };

            makeAlg(out alg);
        }
        public override void destroy(GameTime gameTime) {
            ORGraph.Node.OnNewHoverNode -= updateInfo;
            ORGraph.Edge.OnNewHoverEdge -= updateInfo;
        }

        public override void onEntry(GameTime gameTime) {
            GIOScreen = this;
            neData.setVisible(true);
            onEntryASW();
            onEntryAAW();

            unregisterCameraInput();
            MouseEventDispatcher.OnMouseMotion += onMouseMotion;
            MouseEventDispatcher.OnMousePress += onMousePress;
            MouseEventDispatcher.OnMouseScroll += onMouseWheel;
            KeyboardEventDispatcher.OnKeyPressed += onKeyPress;
            KeyboardEventDispatcher.OnKeyReleased += onKeyRelease;
            KeyboardEventDispatcher.OnKeyPressed += ORGraph.Edge.OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased += ORGraph.Edge.OnKeyRelease;
        }
        public override void onExit(GameTime gameTime) {
            neData.setVisible(false);
            onExitASW();
            onExitAAW();

            MouseEventDispatcher.OnMouseMotion -= onMouseMotion;
            MouseEventDispatcher.OnMousePress -= onMousePress;
            MouseEventDispatcher.OnMouseScroll -= onMouseWheel;
            KeyboardEventDispatcher.OnKeyPressed -= onKeyPress;
            KeyboardEventDispatcher.OnKeyReleased -= onKeyRelease;
            KeyboardEventDispatcher.OnKeyPressed -= ORGraph.Edge.OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased -= ORGraph.Edge.OnKeyRelease;
            unregisterCameraInput();
        }

        public override void update(GameTime gameTime) {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Make Sure We Take A Result
            if(alg.HasResult && !resultTaken) resultTaken = true;

            if(alg.NewStatesCount > 0) {
                // Get Only Number Of Changes At This Time
                int sc = alg.NewStatesCount;

                //Loop Through All Changes
                for(int i = 0; i < sc; i++) {
                    var change = alg.getNextState();
                    actionText.setText(change.CurrentText);
                    if(change.Type.HasFlag(ChangeType.Node)) {
                        //Update The Node
                        graph.Nodes[change.Index].setBColor(change.Color);
                        if(change.HasNumber) { graph.Nodes[change.Index].Distance = change.Number; }
                        if(ORGraph.Node.HoveredNode != null && change.Index == ORGraph.Node.HoveredNode.Index) { updateInfo(graph.Nodes[change.Index]); }
                    }
                    else if(change.Type.HasFlag(ChangeType.Edge)) {
                        //Update The Edge
                        graph.Edges[change.Index].setBColor(change.Color);
                        if(ORGraph.Edge.HoveredLine != null && change.Index == ORGraph.Edge.HoveredLine.Index) { updateInfo(graph.Edges[change.Index]); }
                    }
                }
            }
            if(alg.UseStepping && !alg.PauseByStep) { alg.step(); }
            updateGraph(dt);
            aaw.update(dt);
        }
        public override void draw(GameTime gameTime) {
            game.GraphicsDevice.Clear(
                ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
                GUI.Colors.GraphScreenBackground, 1, 0
                );

            // Draw Graph
            drawGraph(camera.View, camera.Projection, camera.ViewSize);

            // Draw Widgets
            game.SpriteBatch.Begin();
            dataFrame.draw(game.SpriteBatch);
            asw.draw(game.SpriteBatch);
            gData.draw(game.SpriteBatch);
            actionText.draw(game.SpriteBatch);
            aaw.draw(game.SpriteBatch);
            game.SpriteBatch.End();
        }

        public void onMouseMotion(Vector2 loc, Vector2 move) {
            Vector2 w = project(loc);
            graph.checkHover(w);
        }
        public void onMousePress(Vector2 loc, MOUSE_BUTTON button) {
            Vector2 w = project(loc);
            if(IsCamRegistered) { return; }
            if(button == MOUSE_BUTTON.LEFT_BUTTON) {
                if(KeyFlags[KF_BeginAlgorithm]) {
                    ORGraph.Node n = ORGraph.Node.HoveredNode;
                    if(n != null) {
                        if(ORGraph.Node.HoveredNode != null) {
                            resultTaken = !beginAlg(n);
                        }
                    }
                    else {
                        resultTaken = !beginAlg();
                    }
                }
                else { graph.checkSelection(w); }
            }
        }
        public void onMouseWheel(int loc, int dis) {
            if(IsCamRegistered) { return; }
            ORGraph.Edge.cycleHovers(ORGraph.Node.HoveredNode);
        }
        protected override void onMouseWheelCam(int loc, int dis) {
            base.onMouseWheelCam(loc, dis);
            var ms = Mouse.GetState();
            graph.checkHover(project(new Vector2(ms.X, ms.Y)));
            gr.rebuild(game.GraphicsDevice);
        }

        public void onKeyPress(object s, KeyEventArgs args) {
            var ml = KeyboardEventDispatcher.getCurrentModifiers();
            if(ml.IsControlPressed) { registerCameraInput(); }
            switch(args.KeyCode) {
                case Keys.F2: runGIO(GraphIO.IOTaskType.Load, this); break;

                case Keys.B: KeyFlags[KF_BeginAlgorithm] |= true; break;

                case Keys.N:
                    alg.step();
                    break;
                case Keys.T:
                    alg.terminate();
                    actionText.setText("Terminated");
                    break;
                case Keys.P:
                    GIOScreen = null;
                    alg.terminate();
                    State = ScreenState.ChangePrevious;
                    return;
                case Keys.C:
                    alg.terminate();
                    graph.clear(10, 10);
                    gr.rebuild(game.GraphicsDevice);
                    break;

                default: break;
            }
        }
        public void onKeyRelease(object s, KeyEventArgs args) {
            var ml = KeyboardEventDispatcher.getCurrentModifiers();
            if(!ml.IsControlPressed) { unregisterCameraInput(); }
            switch(args.KeyCode) {
                case Keys.B: KeyFlags[KF_BeginAlgorithm] &= false; break;
                default: break;
            }
        }

        protected void deleteGraph() {
            alg.terminate();
            alg.clearStates();
            System.Threading.Thread.Sleep(100);
            ORGraph.clearGraph(graph, 0, 0);
        }

        public virtual void updateInfo(ORGraph.Node n) {
            gData.resetData(n);
        }
        public virtual void updateInfo(ORGraph.Edge e) {
            gData.resetData(e);
        }

        protected abstract void makeAlg(out A a);
        public abstract bool beginAlg(ORGraph.Node node);
        public abstract bool beginAlg();

        AlgSpeedWidget asw;
        void buildASW() {
            asw = new AlgSpeedWidget(
                new Texture2D[] {
                    pixel,
                    game.Content.Load<Texture2D>(@"Textures\Pause"),
                    game.Content.Load<Texture2D>(@"Textures\Play"),
                    game.Content.Load<Texture2D>(@"Textures\Next")
                },
                new AlgSpeedWidget.Info() {
                    BackFrameSize = new Vector2(200, SpeedWidgetHeight),
                    SliderExtentSize = new Vector2(180, SpeedWidgetHeight / 2),
                    SliderSize = new Vector2(20, 35),
                    CBackFrame = GUI.Colors.WidgetBackground,
                    CSEFrame = new Color(22, 22, 22, 255),
                    CSSlow = GUI.Colors.WidgetText,
                    CSFast = new Color(255, 155, 0, 150),
                    CSHover = GUI.Colors.WidgetHighlight
                });
            asw.World = new WidgetFrame(new Vector2(0, game.GraphicsDevice.Viewport.Height - SpeedWidgetHeight), 0);
            asw.OnNewSpeed += onNewSpeed;
            asw.OnNextStep += new Action(asw_OnNextStep);
        }

        void asw_OnNextStep() {
            alg.step();
        }
        void onEntryASW() {
            asw.enable();
            asw.setVisible(true);
            actionText.setVisible(true);
        }
        void onExitASW() {
            asw.disable();
            asw.setVisible(false);
            actionText.setVisible(false);
        }
        void onNewSpeed(float p) {
            if(p <= 0.05f) {
                alg.UseStepping = true;
                alg.PauseByStep = true;
                alg.MilliPauseRate = 1000;

                alg.step();
            }
            else if(p >= 0.95f) {
                alg.UseStepping = false;
                alg.PauseByStep = false;
                alg.MilliPauseRate = 0;
            }
            else {
                alg.UseStepping = true;
                alg.PauseByStep = false;
                int pr = (int)(20000f * Math.Log(-p * p));
                alg.MilliPauseRate = pr > 0 ? pr : 1;
            }
        }

        AvailableActionsWidget aaw;
        void buildAAW() {
            aaw = new AvailableActionsWidget(font,
                new WidgetFrame(new Vector2(-AAWWidth, 0), 0),
                WidgetFrame.Identity, 0.4f,
                new Vector2(AAWWidth, AAWHeight), new Vector2(30, 50),
                GUI.Colors.WidgetBackground, GUI.Colors.WidgetText);
            aaw.Text = AvailableActionText;
        }
        void onEntryAAW() {
            aaw.setVisible(true);
            aaw.hook();
        }
        void onExitAAW() {
            aaw.setVisible(false);
            aaw.unhook();
        }
    }
}
