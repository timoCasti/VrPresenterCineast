using System.Collections.Generic;
using DefaultNamespace;
using InGamePaint;
using Unibas.DBIS.DynamicModelling.Models;
using UnityEngine;

namespace World
{
    /// <summary>
    ///     A representation of a wall, attachable to a gameobject.
    /// </summary>
    public class ExhibitionWall : MonoBehaviour
    {
        public List<Displayal> Displayals = new List<Displayal>();
        public Displayal timeDisplayal;

        /// <summary>
        ///     The wall's data
        /// </summary>
        public DefaultNamespace.VREM.Model.Wall WallData { get; set; }

        /// <summary>
        ///     The model of the wall.
        /// </summary>
        public WallModel WallModel { get; set; }

        /// <summary>
        ///     The Anchor for adding exhibits.
        /// </summary>
        public GameObject Anchor { get; set; }

        public void RestoreDisplayals()
        {
            Displayals.ForEach(d => d.RestorePosition());
        }

        // Method to delete Displayals to replace them.

        public void resetDisplayals()
        {
            // Eventuell sollte man Anchors auch löschen


            int size = Displayals.Count;
            GameObject del;

            for (int i = 0; i < size; i++)
            {
                del = GameObject.Find("Displayal (" + i + ")");
                GameObject.Destroy(del);
                del = GameObject.Find("Anchor (" + i + ")");
                GameObject.Destroy(del);
            }
        }

        public void resetMasterpiece()
        {
            GameObject del;
            // for the canvas atm
            del = GameObject.Find("Displayal (1000)");
            GameObject.Destroy(del);
            del = GameObject.Find("Anchor (1000)");
            GameObject.Destroy(del);
        }


        public void AttachExhibits()
        {
            // TODO Make displayal configurable
            var prefab = ObjectFactory.GetDisplayalPrefab();

            foreach (var e in WallData.exhibits)
            {
                if (e.name != "Masterpiece")
                {
                    var displayal = Instantiate(prefab);
                    displayal.name = "Displayal (" + e.name + ")";
                    displayal.transform.parent = Anchor.transform;
                    var pos = new Vector3(e.position.x, e.position.y, -ExhibitionBuildingSettings.Instance.WallOffset);
                    displayal.transform.localPosition = pos;
                    //displayal.transform.rotation = Quaternion.Euler(ObjectFactory.CalculateRotation(WallData.direction));
                    var rot = Quaternion.Euler(90, 0, 180);
                    displayal.transform.localRotation = rot; // Because prefab is messed up


                    var disp = displayal.gameObject.GetComponent<Displayal>();
                    disp.SetExhibitModel(e);
                    disp.OriginalPosition = pos;
                    disp.OriginalRotation = rot;


                    // Make the Boxcollider trigger in Displayal
                    disp.GetComponent<BoxCollider>().isTrigger = true;
                    Displayals.Add(disp);

                    var image = displayal.transform.Find("Plane").gameObject.AddComponent<ImageLoader>(); // Displayal
                    //ImageLoader image = displayal.AddComponent<ImageLoader>();// ImageDisplayPlane
                    image.ReloadImage(e.GetURLEncodedPath());
                    displayal.transform.localScale = ScalingUtility.convertMeters2PlaneScaleSize(e.size.x, e.size.y);

                    if (e.audio != null)
                    {
                        Debug.Log("added audio to display object");
                        var closenessDetector = displayal.AddComponent<ClosenessDetector>();
                        closenessDetector.url = e.audio;
                    }
                    //Debug.Log(GameObject.Find("Displayal 1000").name+ " 1");
                }
                else
                {
                    // Canvas only gets created if there is no Canvas already 
                    //if (GameObject.Find("Displayal (1000)") == null)
                    if(MyExhibitionBuilder.Masterpiece==false)
                    {
                        //Debug.Log("Didnt find Displayal 1000 ");
                        //GameObject game;
                        //GameObject.Find("Display (1000)");
                        var prefab2 = ObjectFactory.GetCanvasPrefab();
                        var displayalCanvas = Instantiate(prefab2);
                        displayalCanvas.name = "Displayal (" + e.name + ")";
                        displayalCanvas.transform.parent = Anchor.transform;
                        var pos = new Vector3(e.position.x, e.position.y,
                            -ExhibitionBuildingSettings.Instance.WallOffset);
                        displayalCanvas.transform.localPosition = pos;
                        //displayal.transform.rotation = Quaternion.Euler(ObjectFactory.CalculateRotation(WallData.direction));
                        var rot = Quaternion.Euler(90, 0, 180);
                        displayalCanvas.transform.localRotation = rot; // Because prefab is messed up


                        var disp = displayalCanvas.gameObject.GetComponent<Displayal>();
                        disp.SetExhibitModel(e);
                        disp.OriginalPosition = pos;
                        disp.OriginalRotation = rot;

                        displayalCanvas.transform.localScale =
                            ScalingUtility.convertMeters2PlaneScaleSize(e.size.x, e.size.y);

                        Displayals.Add(disp);
                        MyExhibitionBuilder.Masterpiece = true;

                    }
                }

                //Debug.Log("NAMES:  " + displayal.name);
                /*if (!displayal.name.Equals("Displayal (Masterpiece)")) {
                    Destroy(displayal.transform.Find("MyCanvas").gameObject);
                    //Debug.Log("I GOT DESTROYED");
                }*/


                //displayal.transform.Find("Masterpiece").gameObject.AddComponent<Paintable>();

                /*if(!VREPController.Instance.Settings.SpotsEnabled || !e.light){	
                  displayal.transform.Find("Directional light").gameObject.SetActive(false);
                }*/
            }
        }

        public WallOrientation GetOrientation()
        {
            return WallData.GetOrientation();
        }
    }
}