using System.Collections.Generic;

////////////////////////////////////////////////////////////////////
//
// THIS CLASS IS ONLY A SIMULATION OF THE SiSDK
//
////////////////////////////////////////////////////////////////////

namespace SiliconStudio.Presentation.Sample4.Model
{
    /// <summary>
    /// This class simulates a CommandOpManager from the SiSDK.
    /// </summary>
    public class FakeCommandOpManager : ICommandOpManager
    {
        private SimpleModel model;
        private readonly List<int> values = new List<int>();
        private int index;

        public FakeCommandOpManager()
        {
            // default value
            values.Add(0);
        }

        public bool Execute(string commandName, string parameter, bool canUndo)
        {
            if (commandName == "IFSetProperty")
            {
                var value = parameter.Substring("PropertyValue = ".Length);
                model.IntValue = int.Parse(value);
                ++index;
                values.Add(0);
                values[index] = model.IntValue;
                return true;
            }
            return false;
        }

        public bool CanUndo()
        {
            return values.Count > 0;
        }

        public bool CanRedo()
        {
            return index <= values.Count;
        }

        public bool Undo()
        {
            --index;
            model.IntValue = values[index];
            return true;
        }

        public bool Redo()
        {
            ++index;
            model.IntValue = values[index];
            return true;
        }

        public SimpleModel GetSimpleModel()
        {
            model = new SimpleModel();
            return model;
        }
    }
}