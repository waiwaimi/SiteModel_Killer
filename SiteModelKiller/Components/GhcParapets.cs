using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using SiteModelKiller.Common;

namespace SiteModelKiller.Components
{
    public class GhcParapets : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcParapets class.
        /// </summary>
        public GhcParapets()
          : base("Parapets Generator", "Parapets",
              "Generate Parapets",
              "SiteModel", "Building")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Buildings", "Buildings", "The building breps to generate parapets ", GH_ParamAccess.list);
            pManager.AddNumberParameter("ParapetHeight", "ParapetHeight", "The height of the parapets", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("ParapetWidth", "ParapetWidth", "The width (thick) of the parapets", GH_ParamAccess.item, 0.2);
            pManager.AddBooleanParameter("includeBuilding?", "includeBuilding?", "If the Parapets make boolean union with the building", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Parapets", "Parapets", "The generated parapets brep", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> iBuildings = new List<Brep>();
            double iParapetHeight = 0.0;
            double iParapetWidth = 0.0;
            bool iIncludeBuilding = false;

            DA.GetDataList(0, iBuildings);
            DA.GetData(1, ref iParapetHeight);
            DA.GetData(2, ref iParapetWidth);
            DA.GetData(3, ref iIncludeBuilding);
            List<Brep> oParapets = GenBuilding.GenerateParapets(iBuildings,iParapetHeight,iParapetWidth,iIncludeBuilding);

            DA.SetDataList(0, oParapets);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream("SiteModelKiller.icon.GenParapetsIcon.png"))
                {
                    return stream != null ? new System.Drawing.Bitmap(stream) : null;
                }
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("20DFB75C-7205-473F-B71C-11CDB5D7572D"); }
        }
    }
}