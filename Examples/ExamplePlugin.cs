namespace CESDK.Examples
{
    /// <summary>
    /// Example plugin demonstrating the clean, simple API
    /// </summary>
    public class ExamplePlugin : CheatEnginePlugin
    {
        public override string Name => "Clean SDK Example Plugin";

        protected override void OnEnable()
        {
            Console.WriteLine($"Plugin '{Name}' is enabling...");

            // Execute Lua code
            Lua.Execute("print('Hello from the new clean SDK!')");

            // Register a simple C# function
            Lua.RegisterFunction("example_function", ExampleFunction);

            // Register a function with Lua state access
            Lua.RegisterRawFunction("advanced_function", AdvancedFunction);

            // Set up a menu in CE
            Lua.Execute(@"
                local menu = MainForm.Menu
                local topMenu = createMenuItem(menu)
                topMenu.Caption = 'Clean SDK Example'
                menu.Items.insert(MainForm.miHelp.MenuIndex, topMenu)

                local menuItem = createMenuItem(menu)
                menuItem.Caption = 'Test Function'
                menuItem.OnClick = function(sender)
                    example_function()
                    print('Example function executed, status: ' .. plugin_status)
                end
                topMenu.add(menuItem)

                local advancedItem = createMenuItem(menu)
                advancedItem.Caption = 'Advanced Function'
                advancedItem.OnClick = function(sender)
                    local result = advanced_function(42, 'test')
                    print('Advanced function returned: ' .. result)
                end
                topMenu.add(advancedItem)
            ");

            Console.WriteLine("Plugin enabled successfully!");
        }

        protected override void OnDisable()
        {
            Console.WriteLine($"Plugin '{Name}' is disabling...");
            // Clean up resources here if needed
        }

        private void ExampleFunction()
        {
            Console.WriteLine("Example function called from Lua!");

            // You can access Lua here too
            Lua.SetGlobalString("plugin_status", "Function executed");
        }

        private int AdvancedFunction(IntPtr luaState)
        {
            var lua = Lua.Native;

            Console.WriteLine("Advanced function called from Lua!");
            Console.WriteLine($"Lua stack has {lua.GetTop(luaState)} items");

            // Read parameters from Lua stack
            if (lua.GetTop(luaState) >= 2)
            {
                if (lua.IsNumber(luaState, 1))
                {
                    var number = lua.ToInteger(luaState, 1);
                    Console.WriteLine($"Got number: {number}");
                }

                if (lua.IsString(luaState, 2))
                {
                    var text = lua.ToString(luaState, 2);
                    Console.WriteLine($"Got string: {text}");
                }
            }

            // Push return values
            lua.PushString(luaState, "Success");
            lua.PushInteger(luaState, 123);

            return 2; // Number of return values
        }
    }
}