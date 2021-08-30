# MiniEngine

This Work In Progress is the third evolution of MiniEngine, the previous two iterations, located [here](https://github.com/roy-t/MiniRTS), were based on [MonoGame](https://www.monogame.net/). But in the end I found this framework too limiting. 

In this repository I'm working on a version of MiniEngine that builds directly on DirectX 11, using the excellent C# bindings from [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows/). This allows me to use all the modern DirectX and .NET 6 features and write my own content management system that allows hot-reloading. As well working around some other idiosyncrasies that have crept into MonoGame.

For now I'm working on the basics, a light abstraction around DirectX to make it easier to work with. Dependency Injection, logging, and the all important content management. After that I will port most of the features over from the previous version of MiniEngine.