using NFlowGraph.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFlowGraph.Tests
{
    public class TextInputModule : InputModule<string>
    {
        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public TextInputModule(string text)
        {
            _text = text;
            Process = () => _text;
        }
    }
}
