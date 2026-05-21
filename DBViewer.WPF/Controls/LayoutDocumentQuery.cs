using System;
using System.Collections.Generic;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;

namespace DBViewer.WPF.Controls
{
    internal class LayoutDocumentQuery : LayoutDocument
    {
        public string Query { get; private set; }

        public LayoutDocumentQuery(string query)
        {
            this.Query = query;
        }
    }
}
