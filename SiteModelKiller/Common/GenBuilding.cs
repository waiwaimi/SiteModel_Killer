using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino;

namespace SiteModelKiller.Common
{
    public class GenBuilding
    {
        public GenBuilding() { }

        public static bool ConvertToFloor(List<Guid> txt, out List<double> floorNum, out List<Point3d> pts)
        {
            //initialize
            floorNum = new List<double>();
            pts = new List<Point3d>();

            foreach (var text in txt)
            {
                var rhObj = RhinoDoc.ActiveDoc.Objects.Find(text);

                if (rhObj == null)
                    continue;

                var geo = rhObj.Geometry as TextEntity;
                if (geo != null)
                {
                    Point3d basePt = geo.Plane.Origin;
                    string content = geo.PlainText;

                    Regex regex = new Regex(@"\d+(\.\d+)?"); //double or negtive is ok
                    Match match = regex.Match(content);      //first value
                    if (match.Success)
                    {
                        string numberText = match.Value; //number string
                        double value;
                        if (double.TryParse(numberText, out value))
                        {
                            // Success
                            floorNum.Add(value);
                            pts.Add(basePt);
                        }
                        else
                            continue;
                    }
                }
            }

            if (floorNum == null || floorNum.Count == 0)
                return false;
            else
                return true;
        }

        public static List<Brep> GenerateBuildings(List<Guid> floorText, List<Curve> buildingCrvs, double floorHight, GeometryBase baseFrame) 
        {
            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance; //tolerance
                                                                          //Check if the base is planar closed curve or planar brep
            Brep baseSrf = null;

            if (baseFrame is Curve)
            {
                Curve crv = baseFrame as Curve;
                Plane plane;
                bool isPlanar = crv.TryGetPlane(out plane);
                if (!crv.IsClosed || !isPlanar)
                    throw new Exception("The baseFrame must be Planar Closed Curve or planar Surface!!!");

                Brep[] makeSrf = Brep.CreatePlanarBreps(crv, tol);
                baseSrf = makeSrf[0];
            }
            else if (baseFrame is Brep)
            {
                Brep brep = baseFrame as Brep;
                bool isPlanar = brep.Faces.Count == 1 && brep.Faces[0].IsPlanar();
                if (!isPlanar)
                    throw new Exception("The baseFrame must be Planar Closed Curve or planar Surface!!!");

                baseSrf = brep; //model base
            }
            else
            {
                throw new Exception("The baseFrame must be Planar Closed Curve or planar Surface!!!");
            }

            //read building hights
            List<Point3d> pts;
            List<double> floorNum;

            bool convertSuccess = ConvertToFloor(floorText, out floorNum, out pts); //convert ID -> num & pt

            if (!convertSuccess)
                throw new Exception("Cannot Read the Building Hight!!");

            //generate buildings bottom
            List<Brep> cutters = new List<Brep>();

            foreach (var c in buildingCrvs)
            {
                Brep cutter = Brep.CreateFromSurface(Surface.CreateExtrusion(c, Vector3d.ZAxis));
                cutters.Add(cutter);
            }

            Brep[] splited = baseSrf.Split(cutters, tol);
            Brep[] cleaned = splited.Where(b => b != null).ToArray(); //cull null

            //find
            List<Brep> matchedBreps = new List<Brep>();
            List<Point3d> matchedPoints = new List<Point3d>();
            List<int> matchedPtIndex = new List<int>();

            foreach (Brep brep in cleaned)
            {
                BrepFace face = brep.Faces[0];

                for (int i = 0; i < pts.Count; i++)
                {
                    Point3d pt = pts[i];

                    double u, v;
                    if (face.ClosestPoint(pt, out u, out v))
                    {
                        var result = face.IsPointOnFace(u, v, tol);

                        if (result == PointFaceRelation.Interior || result == PointFaceRelation.Boundary)
                        {
                            matchedBreps.Add(brep);
                            matchedPoints.Add(pt);
                            matchedPtIndex.Add(i);
                            break;
                        }
                    }
                }
            }

            //generate buildings
            //List<double> test = new List<double>();
            List<Brep> buildings = new List<Brep>();

            for (int h = 0; h < matchedBreps.Count; h++)
            {
                double buildingH = floorNum[matchedPtIndex[h]] * floorHight;
                Vector3d extrusion = Vector3d.ZAxis * buildingH;

                Brep bBottom = matchedBreps[h]; //bottom

                Brep btop = matchedBreps[h].DuplicateBrep(); //topCap
                btop.Translate(extrusion);

                BrepFace face = bBottom.Faces[0]; //side
                BrepLoop outerLoop = face.Loops.First(l => l.LoopType == BrepLoopType.Outer);
                Curve outerCrv = outerLoop.To3dCurve();
                Surface sideSurf = Surface.CreateExtrusion(outerCrv, extrusion);
                Brep sideBrep = sideSurf.ToBrep();

                //join to building
                List<Brep> pieces = new List<Brep> { bBottom, btop, sideBrep };
                Brep[] joined = Brep.JoinBreps(pieces, tol);
                Brep buildingBody = joined[0];

                buildings.Add(buildingBody);
            }

            return buildings;
        }

        public static List<Brep> GenerateParapets(List<Brep> buildings, double parapetHight, double thick) 
        {
            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance; //tolerance

            List<Brep> parapets = new List<Brep>();
            List<Curve> parapetBounds = new List<Curve>();

            //parapet boundary
            foreach (var b in buildings)
            {
                BrepFace topFace = null;
                double maxZ = double.MinValue; //finding highest face

                foreach (BrepFace r in b.Faces)
                {
                    BoundingBox bbRoof = r.GetBoundingBox(true);

                    Plane plane;
                    if (r.IsPlanar() && r.TryGetPlane(out plane))
                    {
                        double dotZ = plane.Normal * Vector3d.ZAxis; //
                        if (Math.Abs(dotZ) > 0.99 && bbRoof.Max.Z > maxZ)
                        {
                            maxZ = bbRoof.Max.Z;
                            topFace = r;
                        }
                    }
                }

                if (topFace != null)
                {
                    BrepLoop outerLoop = topFace.Loops.First(l => l.LoopType == BrepLoopType.Outer);
                    Curve bound = outerLoop.To3dCurve();
                    parapetBounds.Add(bound);
                }
            }

            //extrude & offset
            List<Curve> test = new List<Curve>();

            foreach (Curve outline in parapetBounds)
            {
                //offset direction
                Plane plane;
                if (!outline.IsClosed || !outline.TryGetPlane(out plane))
                    continue;

                Curve[] inner = outline.Offset(plane, thick, tol, CurveOffsetCornerStyle.Sharp);

                if (inner == null || inner.Length == 0)
                    continue;

                //check offset
                AreaMassProperties amp1 = AreaMassProperties.Compute(outline);
                AreaMassProperties amp2 = AreaMassProperties.Compute(inner[0]);

                if (amp1 == null || amp2 == null)
                {
                    continue;
                }
                else if (amp2.Area > amp1.Area)
                {
                    thick = -thick; //reverse
                    inner = outline.Offset(plane, thick, tol, CurveOffsetCornerStyle.Sharp);
                }

                Vector3d extrusion = Vector3d.ZAxis * parapetHight;

                Curve[] bottom = new Curve[] { outline, inner[0] };
                Brep[] bottomCap = Brep.CreatePlanarBreps(bottom, tol); //bottom cap

                Curve outerTop = outline.DuplicateCurve();
                outerTop.Translate(extrusion);

                Surface outerWall = Surface.CreateExtrusion(outline, extrusion);
                Brep outerBrep = outerWall.ToBrep();

                Surface innerWall = Surface.CreateExtrusion(inner[0], extrusion);
                Brep innerBrep = innerWall.ToBrep();

                Curve innerTop = inner[0].DuplicateCurve();
                innerTop.Translate(extrusion);

                Curve[] top = new Curve[] { outerTop, innerTop };
                Brep[] topCap = Brep.CreatePlanarBreps(top, tol); //top cap

                List<Brep> allParts = new List<Brep>();
                allParts.Add(outerBrep);
                allParts.Add(innerBrep);
                allParts.AddRange(topCap);
                allParts.AddRange(bottomCap);
                Brep[] result = Brep.JoinBreps(allParts, tol);

                parapets.AddRange(result);
            }

            return parapets;
        }
    }


}
