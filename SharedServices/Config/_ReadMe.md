Config Overview
===============

The Config structure typically maps to a directory like this:

%appdata%\\Company\\AppName\\group.xml

Where group is the settings for a particular class (e.g. MainFormTrader) or sometimes a particular instance of a class (
e.g. ChartES1).

Of course, it doesn't have to be a file -- it could be a database or any persistent storage.

IConfigJsonIOFile is the interface for the actual saving and loading

UserControls are special and support the IConfigParent interface. The convention is to store them under a name like
“mainform.servercontrol”

Note that namespace migration examples are inserted into CoreJsonContractResolver by Charts.Program.cs