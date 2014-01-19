How Data Is Formatted:

Each Graph Must Be Loaded With A File Of Nodes And Of Edges.
Optionally, A Grid File May Be Loaded To Specify Exact Grid Size
With It Lower-left Point Starting At (0,0).

=================
Node File Format:
=================

Count[N]
				- N Is The Number Of Nodes To Be Read

Node[Index]
				- Index Of The Node In Range [0,N-1]
nd[X|Y|Z-Depth|Radius|Flags]
				- (X,Y) Coordinates
				- Z-Depth Is Between (0,-1)
				- Radius Is The Radius Of The Node
				- A Byte Flag (Usually 0)
ndcolor[R|G|B|A]
				- RGBA Color With Integer Values Between [0,255]





=================
Edge File Format:
=================

Count[E]
				- E Is The Number Of Edges To Be Read

Edge[Index|Start|End|AsEdge]
				- Index Of The Edge In Range [0,E-1]
				- Index Of Start Node [0,N-1]
				- Index Of End Node [0,N-1]
				- (true/false) For If It Acts As An Edge Or Arc
ed[Weight|Width|Flags]
				- Weight/Capacity Of The Edge
				- How Wide An Edge Looks
				- A Byte Flag (Usually 0)
edcolor[R|G|B|A]
				- RGBA Color With Integer Values Between [0,255]


=================
Grid File Format:
=================
Grid	[Width|Height]
				- Width, Height Of Grid (Must Be Greater Than The Maximum X,Y Coordinates Of The Nodes)
GMap	[PixelWidth|PixelHeight|Zoom|Resolution|Address]
				- Pixel Width/Height Of Retrieved Bitmap
				  (Internet Must Be On For This Or Potential To Freeze If It's Crappy/Slow/Google Maps Is Down)
				- Zoom Value [1,14] With Greater Numbers Being Greater Zoom
				- Resolution [1 or 2] With Will Respectively Multiply Size Of
				  The Bitmap Retrieved, But Keep Same View Bounds
				- The Address Of The Center Of The Grid


=======================
Other:
=======================
Comments Can Be Written Like

#Comment Here
#Another Comment
#The Pound Sign Is Important
=======================
Macros Can Be Set And Used Like So:

#FILE BEGIN
%SET [MACRO1|Hi]
%SET [STR|@MACRO1@ Bob]

Message [@STR@]

%UNSET [MACRO1]
%UNSET [STR]
#FILE END

And This Will Be Interpreted:

Message [Hi Bob]

This Is Good For Making Grid Files Like So:

#Use This To Set The Center Of The Map
%SET [MapCenter|Harrisburg, PN]

Grid	[2048|2048]
GMap	[1024|1024|7|2|@MapCenter@]

%UNSET [MapCenter]

But, Be Careful. If A Macro Is Not Unset After A File
Is Read, It Will Exist In Later Files.
=========


======================================
Edges/Nodes Must Be Written Consecutively:

Accepted:
Count[5]
Edge[...]
ed[...]
edcolor[...]
Edge[...]
ed[...]
edcolor[...]
Edge[...]
ed[...]
edcolor[...]
Edge[...]
ed[...]
edcolor[...]
Edge[...]
ed[...]
edcolor[...]

Not Good:
Count[5]
Edge[...]
Edge[...]
Edge[...]
Edge[...]
Edge[...]
ed[...]
edcolor[...]
ed[...]
edcolor[...]
ed[...]
edcolor[...]
ed[...]
edcolor[...]
ed[...]
edcolor[...]


