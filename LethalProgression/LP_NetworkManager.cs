using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LethalProgression
{
    {
		[HarmonyPrefix]
		[HarmonyPatch(typeof(StartOfRound), "ResetShip")]
		public static void storePXP(StartOfRound __instance, out double storedPXP)
		{
            InternalClass LCXP = new LC_XP();
			storedPXP = LCXP.xpPersistent.Value;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(StartOfRound), "ResetShip")]
		public static void loadUnlocked(StartOfRound __instance, storedPXP)
		{
            int convertPXP = (int)storedPXP;
            InternalClass LCXP = new LC_XP();
		    LCXP.AddXPServerRPCStart(convertPXP);
		}
    [HarmonyPatch]
    internal class LP_NetworkManager
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init()
        {
            if (xpNetworkObject != null)
                return;

            xpNetworkObject = (GameObject)LethalPlugin.skillBundle.LoadAsset("LP_XPHandler");
            xpNetworkObject.AddComponent<LC_XP>();
            NetworkManager.Singleton.AddNetworkPrefab(xpNetworkObject);
        }

        public static GameObject xpNetworkObject;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = Object.Instantiate(xpNetworkObject, Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
                xpInstance = networkHandlerHost.GetComponent<LC_XP>();
                LethalPlugin.Log.LogInfo("XPHandler Initialized.");
            }
        }

        public static LC_XP xpInstance;
    }
}
