using HarmonyLib;
using MelonLoader;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players;
using RumbleModUI;

namespace BucketEquipper
{
    [HarmonyPatch(typeof(Il2CppRUMBLE.Environment.Minigames.ParkMinigame), "OnMiniGameEnded")]
    public static class Patch
    {
        private static void Postfix()
        {
            try
            {
                foreach (Player player in PlayerManager.instance.AllPlayers)
                {
                    if (player.Controller.controllerType == Il2CppRUMBLE.Players.ControllerType.Local)
                    {
                        if (BucketEquipperClass.showForSelf)
                        {
                            player.Controller.gameObject.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (BucketEquipperClass.showForOthers)
                        {
                            player.Controller.gameObject.transform.GetChild(7).GetChild(0).gameObject.SetActive(true);
                        }
                    }
                }
            }
            catch { }
        }
    }

    public class BucketEquipperClass : MelonMod
    {
        private string currentScene = "Loader";
        private bool sceneChanged = false;
        private int playerCount;
        public static bool showForSelf, showForOthers;
        UI UI = UI.instance;
        private Mod BucketEquipper = new Mod();

        public override void OnLateInitializeMelon()
        {
            BucketEquipper.ModName = "Equips THE Bucket";
            BucketEquipper.ModVersion = "2.0.1";
            BucketEquipper.SetFolder("BucketEquipper");
            BucketEquipper.AddDescription("Description", "Description", "Toggles Buckets on Everyone", new Tags { IsSummary = true });
            BucketEquipper.AddToList("Self", false, 0, "Turns On/Off Self Bucket", new Tags { });
            BucketEquipper.AddToList("Others", true, 0, "Turns On/Off Others Buckets", new Tags { });
            BucketEquipper.GetFromFile();
            showForSelf = (bool)BucketEquipper.Settings[1].Value;
            showForOthers = (bool)BucketEquipper.Settings[2].Value;
            BucketEquipper.ModSaved += Save;
            UI.instance.UI_Initialized += UIInit;
        }

        public void UIInit()
        {
            UI.AddMod(BucketEquipper);
        }

        public void Save()
        {
            showForSelf = (bool)BucketEquipper.Settings[1].Value;
            showForOthers = (bool)BucketEquipper.Settings[2].Value;
            equipBuckets();
        }

        //run every update
        public override void OnFixedUpdate()
        {
            if (sceneChanged)
            {
                equipBuckets();
                sceneChanged = false;
            }
            else if ((currentScene != "Loader") && (currentScene != "Gym"))
            {
                if (playerCount != PlayerManager.instance.AllPlayers.Count)
                {
                    equipBuckets();
                }
            }
        }

        private void equipBuckets()
        {
            try
            {
                int i = 0;
                foreach (Player player in PlayerManager.instance.AllPlayers)
                {
                    if (player.Controller.controllerType == Il2CppRUMBLE.Players.ControllerType.Local)
                    {
                        if (showForSelf)
                        {
                            player.Controller.gameObject.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);
                        }
                        else
                        {
                            player.Controller.gameObject.transform.GetChild(6).GetChild(0).gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (showForOthers)
                        {
                            player.Controller.gameObject.transform.GetChild(7).GetChild(0).gameObject.SetActive(true);
                        }
                        else
                        {
                            player.Controller.gameObject.transform.GetChild(7).GetChild(0).gameObject.SetActive(false);
                        }
                    }
                    playerCount = i;
                    i++;
                }
            }
            catch {}
        }

        //called when a scene is loaded
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //update current scene
            currentScene = sceneName;
            sceneChanged = true;
        }
    }
}
