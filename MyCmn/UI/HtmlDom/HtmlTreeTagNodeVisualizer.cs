using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MyCmn;
using System.Data.Common;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Web.UI;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.VisualStudio.DebuggerVisualizers;
using MyCmn.Visualizer;

namespace MyCmn.Visualizer
{
    public class HtmlTreeTagNodeVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService,
                IVisualizerObjectProvider objectProvider)
        {
            // TODO: Get the object to display a visualizer for.
            //       Cast the result of objectProvider.GetObject() 
            //       to the type of the object being visualized.
            var myTable =  objectProvider.GetObject();
            // TODO: Display your view of the object.
            //       Replace displayForm with your own custom Form or Control.
            var displayForm = new HtmlTreeTagNodeViewerForm(myTable as HtmlTreeTagNode);
            windowService.ShowDialog(displayForm);
        }
    }
}
