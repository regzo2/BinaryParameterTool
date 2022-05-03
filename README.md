# Binary Parameter Tool
### Simple Unity Editor tool that helps create Animation Layers that a Binary Parameter can interface with for OSC Applications.

![binary parameter tool showcase](https://user-images.githubusercontent.com/74634856/148631235-9653f177-b818-4241-b13f-86892bfd6317.gif)

# **What are Binary Parameters?**
Binary parameters is the demonination given to a group of bool parameters that use a Base 2 counting system in order to replicate incremental parameters like floats or ints for VRChat Expression Parameters. The biggest draw to binary parameters is their ability to scale the resolution of counting: Currently a float parameter has a fixed size of 8 bits and can only be partitioned within that specific parameter without retaining any separation, but using a binary parameter allows you to emulate a float and resize it at any time inside an Animator.

# **How can I implement Binary Parameters into my OSC application?**
Binary Parameters follow a base 2 counting format, and applications can implement them to follow how the Binary Parameter Tool creates the bool parameter system

# **Instructions**
Simply install the included .unitypackage in your Unity project! This tool will be available on the toolbar under Tools / VRCFaceTracking.
* Tooltips available on each setting that explain what to input or do!
