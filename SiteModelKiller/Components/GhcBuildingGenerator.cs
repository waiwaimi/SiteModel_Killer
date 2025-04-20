using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using SiteModelKiller.Common;

namespace SiteModelKiller.Components
{
    public class GhcBuildingGenerator : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcBuildingGenerator class.
        /// </summary>
        public GhcBuildingGenerator()
          : base("BuildingGenerator", "BuildingGenerator",
              "Generate the buildings on your site model.",
              "SiteModel", "Building")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Text", "Text", "Text including floors...ect.", GH_ParamAccess.list);
            pManager.AddCurveParameter("BuildingCrvs", "BuildingCrvs", "Building curves", GH_ParamAccess.list);
            pManager.AddNumberParameter("FloorHeight", "FloorHeight", "The height of each floor", GH_ParamAccess.item, 3.0);
            pManager.AddGeometryParameter("BaseFrame", "BaseFrame", "The base frame for the site model", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Building", "Building", "The generated building brep", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Guid> iText = new List<Guid>();
            List<Curve> iBuildingCrvs = new List<Curve>();
            double iFloorHeight = 0.0;
            GeometryBase iBaseFrame = null;

            DA.GetDataList(0,  iText);
            DA.GetDataList(1,  iBuildingCrvs);
            DA.GetData(2, ref iFloorHeight);
            DA.GetData(3, ref iBaseFrame);

            List<Brep> oBuildings = GenBuilding.GenerateBuildings(iText, iBuildingCrvs, iFloorHeight, iBaseFrame);
            DA.SetDataList(0, oBuildings);

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
                //return null;
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream("SiteModelKiller.icon.GenBuildingsIcon.png"))
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
            get { return new Guid("424E07F3-EA2A-43B8-A7CE-F58ED11162DF"); }
        }
    }
}