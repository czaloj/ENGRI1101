using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA2D.Framework
{
    public class SpriteTextureOptions
    {
        public Vector2 textureSize;

        public Vector2 viewSize;
        private Vector2 scale;
        public Vector2 Scale { get { return scale; } }

        public Vector2 pivotCenterOffset;
        private Vector2 texPivot;
        public Vector2 TexturePivot { get { return texPivot; } }

        public SpriteTextureOptions(Vector2 textureSize)
        {
            this.textureSize = textureSize;
            viewSize = Vector2.One;
            scale = viewSize / textureSize;
            pivotCenterOffset = Vector2.Zero;
            texPivot = textureSize * (pivotCenterOffset + Vector2.One) / 2;
        }
        public SpriteTextureOptions(Vector2 textureSize, Vector2 viewSize, Vector2 pivotCenterOffset)
        {
            this.textureSize = textureSize;
            this.viewSize = viewSize;
            scale = viewSize / textureSize;
            this.pivotCenterOffset = pivotCenterOffset;
            texPivot = textureSize * (pivotCenterOffset + Vector2.One) / 2;
        }

        public void setViewSize(Vector2 viewSize)
        {
            this.viewSize = viewSize;
            scale = viewSize / textureSize;
        }
        public void setPivotCenterOffset(Vector2 pivotCenterOffset)
        {
            this.pivotCenterOffset = pivotCenterOffset;
            texPivot = textureSize * (pivotCenterOffset + Vector2.One) / 2;
        }
        public void recalculate(Vector2 textureSize)
        {
            this.textureSize = textureSize;
            scale = viewSize / this.textureSize;
            texPivot = this.textureSize * (pivotCenterOffset + Vector2.One) / 2;
        }
    }
}
