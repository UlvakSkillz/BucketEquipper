using HarmonyLib;
using MelonLoader;
using RUMBLE.Managers;
using RUMBLE.Players;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BucketEquipper
{
    [HarmonyPatch(typeof(RUMBLE.Environment.Minigames.ParkMinigame), "OnMiniGameEnded")] // Example is the name of the class
    public static class Patch
    {
        private static void Postfix()
        {
            PlayerManager playerManager = GameObject.Find("Game Instance/Initializable/PlayerManager").gameObject.GetComponent<PlayerManager>();
            PlayerController playerController = playerManager.localPlayer.Controller;
            bool showForSelf = false;
            bool showForOthers = false;
            if (File.Exists(@"UserData\BucketEquipper\Settings.txt"))
            {
                try
                {
                    string[] fileContents = File.ReadAllLines(@"UserData\BucketEquipper\Settings.txt");
                    if (fileContents[1].ToLower() == "true")
                    {
                        showForSelf = true;
                    }
                    else
                    {
                        showForSelf = false;
                    }
                    if (fileContents[3].ToLower() == "true")
                    {
                        showForOthers = true;
                    }
                    else
                    {
                        showForOthers = false;
                    }
                }
                catch
                {
                    MelonLogger.Error("Error Reading Settings File");
                }
            }
            else
            {
                showForSelf = true;
                showForOthers = true;
            }
            try
            {
                foreach (Player player in playerManager.AllPlayers)
                {
                    if (player.Controller.controllerType == ControllerType.Local)
                    {
                        if (showForSelf)
                        {
                            player.Controller.gameObject.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (showForOthers)
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
        private string settingsFile = @"UserData\BucketEquipper\Settings.txt";
        private string FILEPATH = @"UserData\BucketEquipper";
        private string FILENAME = @"Settings.txt";
        private string currentScene = "";
        private bool gymInitRan = false;
        private bool sceneChanged = false;
        public PlayerManager playerManager;
        private int playerCount;
        private bool showForSelf, showForOthers;

        public override void OnLateInitializeMelon()
        {
            if (!File.Exists(settingsFile))
            {
                MelonCoroutines.Start(CheckIfFileExists(FILEPATH, FILENAME));
            }
            else
            {
                try
                {
                    string[] fileContents = File.ReadAllLines(settingsFile);
                    if (fileContents[1].ToLower() == "true")
                    {
                        showForSelf = true;
                    }
                    else
                    {
                        showForSelf = false;
                    }
                    if (fileContents[3].ToLower() == "true")
                    {
                        showForOthers = true;
                    }
                    else
                    {
                        showForOthers = false;
                    }
                    MelonLogger.Msg("Settings Loaded | Self: " + showForSelf + " | Others: " + showForOthers);
                }
                catch
                {
                    MelonLogger.Error("Error Reading Settings File");
                }
            }
        }

        public IEnumerator CheckIfFileExists(string filePath, string fileName)
        {
            if (!File.Exists($"{filePath}\\{fileName}"))
            {
                if (!Directory.Exists(filePath))
                {
                    MelonLogger.Msg($"Folder Not Found, Creating Folder: {filePath}");
                    Directory.CreateDirectory(filePath);
                }
                if (!File.Exists($"{filePath}\\{fileName}"))
                {
                    MelonLogger.Msg($"Creating File {filePath}\\{fileName}");
                    File.Create($"{filePath}\\{fileName}");
                }
                showForOthers = true;
                showForSelf = false;
                for (int i = 0; i < 60; i++) { yield return new WaitForFixedUpdate(); }
                string[] newFileText = new string[4];
                newFileText[0] = "Show On Self:";
                newFileText[1] = "False";
                newFileText[2] = "Show on Others:";
                newFileText[3] = "True";
                File.WriteAllLines($"{filePath}\\{fileName}", newFileText);
            }
            yield return null;
        }

        //run every update
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (sceneChanged)
            {
                if ((currentScene == "Gym") && (!gymInitRan))
                {
                    try
                    {
                        playerManager = GameObject.Find("Game Instance/Initializable/PlayerManager").gameObject.GetComponent<PlayerManager>();
                        gymInitRan = true;
                    }
                    catch(Exception e)
                    {
                        MelonLogger.Error(e.Message);
                        return;
                    }
                }
                if (gymInitRan)
                {
                    equipBuckets();
                }
                sceneChanged = false;
            }
            else if ((currentScene != "") && (currentScene != "Loader"))
            {
                if (playerCount != playerManager.AllPlayers.Count)
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
                foreach (Player player in playerManager.AllPlayers)
                {
                    if (player.Controller.controllerType == ControllerType.Local)
                    {
                        if (showForSelf)
                        {
                            player.Controller.gameObject.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (showForOthers)
                        {
                            player.Controller.gameObject.transform.GetChild(7).GetChild(0).gameObject.SetActive(true);
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
            base.OnSceneWasLoaded(buildIndex, sceneName);
            //update current scene
            currentScene = sceneName;
            sceneChanged = true;
        }
    }
}
