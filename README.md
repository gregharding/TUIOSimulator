# TUIOSimulator v1.0

**Simple Unity/C# TUIO v1.1 simulator for OS X, Windows, iOS, and Android.**

Send and receive [TUIO](http://www.tuio.org/) cursors and object data to and from TUIO server/client apps. Can be built for any platforms that Unity 5.x and TouchScript run on. (Tested on OS X, Win 7/10, iOS.)

Defaults to sending TUIO to 127.0.0.1:3333 and receiving incoming TUIO on port 33333.

Supports cursors (touches/mouse) and objects (markers). Does not support blobs yet. The simulator currently contains 8 objects/markers but you can easily add more and customise them, etc.

**Usage**

Touch or mouse-click on the surface to create and send TUIO cursors. Touch or mouse-down on an object and drag them around the surface to create and send TUIO objects. Objects can be rotated using two-finger gestures or by ALT + mouse-clicking to place the first touch, then mouse-down again for the second touch.

The simulator is chainable to another instance of itself or other TUIO applications. It will happily receive and render TUIO cursor and object information (on the local listening port) and then send it out again (to the destination server:port).

Can be used with TUIOListener ([https://github.com/gregharding/TUIOListener](https://github.com/gregharding/TUIOListener)), a command-line TUIO v1.1 / OSC v1.1 network listener.

Libraries:
* [https://github.com/TouchScript/TouchScript](https://github.com/TouchScript/TouchScript)

**Todo**

The simulator is intended to be used in landscape orientation - portrait and other general resizing might require some tweaks.

The hit areas for objects are currently larger than their visible sprites to make it easier to use on iOS when moving and using two-finger gestures for rotation. These expanded hit areas are also used to determine the active objects on the surface so objects will remain active until they're a bit further away from the surface than the sprite shows.

**Download**

Download executables for OS X and Windows at [https://github.com/gregharding/TUIOSimulator/releases](https://github.com/gregharding/TUIOSimulator/releases).

**Author**

Greg Harding [http://www.flightless.co.nz](http://www.flightless.co.nz)

Copyright 2016 Flightless Ltd

**License**

> The MIT License (MIT)
> 
> Copyright (c) 2016 Flightless Ltd
> 
> Permission is hereby granted, free of charge, to any person obtaining
> a copy of this software and associated documentation files (the
> "Software"), to deal in the Software without restriction, including
> without limitation the rights to use, copy, modify, merge, publish,
> distribute, sublicense, and/or sell copies of the Software, and to
> permit persons to whom the Software is furnished to do so, subject to
> the following conditions:
> 
> The above copyright notice and this permission notice shall be
> included in all copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
> EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
> MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
> NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
> BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
> ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
> CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
> SOFTWARE.
