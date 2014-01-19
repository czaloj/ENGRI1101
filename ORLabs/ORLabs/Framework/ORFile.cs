using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ZLibrary.Graphs;

namespace ORLabs.Framework {
    public class ORFile<G, E, N, ED, ND>
        where G : Graph<E, N>, new()
        where E : Edge<N>, new()
        where N : Node<E>, new() {
        string nodeFile;
        string edgeFile;
        string miscFile;

        G graph;
        public G Graph { get { return graph; } }

        public ORFile(G g = null) {
            graph = g ?? new G();
        }
        public ORFile(string n, string e, string m = null, G g = null)
            : this(g) {
            setFiles(n, e, m);
        }

        public void setFiles(string n, string e, string m = null) {
            nodeFile = n;
            edgeFile = e;
            miscFile = m;
        }

        public void read() {
            if(!string.IsNullOrWhiteSpace(nodeFile) && File.Exists(nodeFile)) {
                using(TokenStream s = new TokenStream(new StreamReader(nodeFile))) {
                    graph.readNodes(s);
                }
            }
            else { throw new Exception("There Must Be A Node File"); }
            if(!string.IsNullOrWhiteSpace(edgeFile) && File.Exists(edgeFile)) {
                using(TokenStream s = new TokenStream(new StreamReader(edgeFile))) {
                    graph.readEdges(s);
                }
            }
            else { throw new Exception("There Must Be An Edge File"); }
            if(!string.IsNullOrWhiteSpace(miscFile) && File.Exists(miscFile)) {
                using(TokenStream s = new TokenStream(new StreamReader(miscFile))) {
                    graph.readMisc(s);
                }
            }
        }
        public void write() {
            if(!string.IsNullOrWhiteSpace(nodeFile)) {
                using(TokenStream s = new TokenStream(new StreamWriter(nodeFile))) {
                    graph.writeNodes(s);
                    s.flushTokens();
                }
            }
            else { throw new Exception("There Must Be A Node File"); }
            if(!string.IsNullOrWhiteSpace(edgeFile)) {
                using(TokenStream s = new TokenStream(new StreamWriter(edgeFile))) {
                    graph.writeEdges(s);
                    s.flushTokens();
                }
            }
            else { throw new Exception("There Must Be An Edge File"); }
            if(!string.IsNullOrWhiteSpace(miscFile)) {
                using(TokenStream s = new TokenStream(new StreamWriter(miscFile))) {
                    graph.writeMisc(s);
                    s.flushTokens();
                }
            }
        }
    }

    public class ORGraphFile : ORFile<ORGraph, ORGraph.Edge, ORGraph.Node, EdgeData, NodeData> {
        public ORGraphFile(ORGraph g = null)
            : base(g) {
        }
        public ORGraphFile(string n, string e, string m = null, ORGraph g = null)
            : base(n, e, m, g) {
        }
    }
}
