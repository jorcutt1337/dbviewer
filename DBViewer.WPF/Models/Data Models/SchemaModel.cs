using System;
using System.Collections.Generic;
using System.Text;

namespace DBViewer.WPF.Models
{
    /// <summary>
    /// Model Used For Dynamically Building SQL Queries Based On Schema Information
    /// </summary>
    public class SchemaModel
    {
        public string TableName { get; set; } = string.Empty;

        public List<SchemaViewModel> Columns { get; set; } = new List<SchemaViewModel>();

        public List<RelationViewModel> RelationsDown { get; set; } = new List<RelationViewModel>();
        public List<RelationViewModel> RelationsUp { get; set; } = new List<RelationViewModel>();
    }
}
