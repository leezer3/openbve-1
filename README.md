openBVE
=======
"A license-free, open source, free of charge train driving simulator. Runs most BVE content from third party providers."

This is a continuation of openBVE 1.4.2 and 1.4.3 (managed content is hidden but source code is still alive).

------------
New features
------------
* SDL2# & OpenTK as replacement for deprecated Tao
* New security (train plugin) keys (M-Z)
* Uses SDL scancodes instead of SDL keycodes
  * Note: WinForms controls editor now shows both physical and virtual keys.

API changes
-----------
* Split Interface class to smaller classes:
  * BlackBox - train logs
  * Controls
  * Conversions - the TryParse... functions
  * Debug - program's error log
  * Hud
  * Options - note: CurrentOptions is renamed to Current
  * Strings - Translation engine
* Put all vectors to OpenBveApi
  * Double- and Single- precision vectors are provided by default
* Joystick class now provides a SDL Guid.

-----
TODOs
-----
Here are some of the goals of this repository (they aren't sorted by priority and part of them is also in roadmap):

* [Roadmap](https://sites.google.com/site/openbvesim/Roadmap) goals
* Change internal representation of route to allow multi-track levels
  * Implement BVE 5 compatibility
  * Implement multiplayer
  * Implement modular renderer + backends for different OpenGL versions (or for Direct3D)
    * *Getting rid of immediate mode rendering should offer a performance boost*
* Improve documentation level
* Implement OpenGL menu *(cross-platformness)* - complete GUI redesign
* Implement gamepad+trackball support and allow mouse buttons to be assigned too
* *Bring managed content back to life* **- Not in actual state, see [this thread on BVEWorldWide](http://bveworldwide.unlimitedboard.com/t439p120-development-discontinued-discussing-the-direction-to-go)**

And maybe in distant future:

* Mobile version (primarily for Android, maybe iOS, if some Mac guy helps me)
  * Touchscreen interface

------------------------
Dedication to the public
------------------------
This program, along with all documentation provided, is dedicated
to the public. I do not pose any restrictions on how this material
can be used, and explicitly encourage redistribution and
modification for any purpose.

-----------------------
Abandoning this project
-----------------------
I'm not planning to leave this project at any time, but because I'm a student, I might not have enough free time to actively maintain this project, so please be patient if I don't respond.

--------
Homepage
--------

* Community webpages: <https://sites.google.com/site/openbvesim/home>
* Official webpages (Odakyufan): <http://odakyufan.zxq.net/openbve/>
* Unofficial webpages of this project (not yet created): <http://jakubvanekpc.jecool.net/openbve/>
