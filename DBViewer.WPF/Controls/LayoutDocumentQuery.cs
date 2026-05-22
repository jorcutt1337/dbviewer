using System;
using System.Collections.Generic;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;

namespace DBViewer.WPF.Controls
{
    internal class LayoutDocumentQuery : LayoutDocument
    {
        #region Properties

        public string Query { get; private set; }

        #endregion

        #region Initialization

        public LayoutDocumentQuery(string query)
        {
            this.Query = query;
        }

        #endregion
    }
}
