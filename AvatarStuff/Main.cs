using MelonLoader;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;
using VRC.UI;

namespace AvatarStuff
{
    public class Main : MelonMod
    {
        public static Player me;
        public static bool emmCheck = false;
        public static float lastVal = 0f;
        public static float lastRotation = 0f;
        public static bool skipRotationFix = false;
        public static GameObject baseButton;

        public static void Log(string log, bool debug=false)
        {
            if (debug)
            {
                if (!Imports.IsDebugMode()) return;
                log = "[DEBUG] " + log;
            }
            MelonModLogger.Log(log);
        }

        public static void WarnLog(string log, bool debug = false)
        {
            if (debug)
            {
                if (!Imports.IsDebugMode()) return;
                log = "[DEBUG] " + log;
            }
            MelonModLogger.LogWarning(log);
        }

        public static void ErrLog(string log, bool debug = false)
        {
            if (debug)
            {
                if (!Imports.IsDebugMode()) return;
                log = "[DEBUG] " + log;
            }
            MelonModLogger.LogError(log);
        }

        public override void VRChat_OnUiManagerInit()
        {
            string DoubleStandards = "emmVRC can be obfuscated but this can't be? Thats a bit shitty.";

            baseButton = Sub.GetUiObject("/Avatar/Stats Button");

            GameObject listObj = Sub.CreateObjectFrom(Sub.GetUiObject("/Avatar").transform.Find("Vertical Scroll View/Viewport/Content").transform.Find("Favorite Avatar List").gameObject);
            listObj.transform.Find("Button").GetComponentInChildren<Text>().text = "Public Avatar List";
            listObj.transform.SetSiblingIndex(0);
            listObj.SetActive(false);

            UiAvatarList listListObj = listObj.GetComponent<UiAvatarList>();
            listListObj.category = (UiAvatarList.EnumNPublicSealedvaInPuMiFaSpClPuLi9vUnique)4;
            listListObj.StopAllCoroutines();

            Sub.CreateAvatarButton("Get Public Avatars", -(baseButton.GetComponent<RectTransform>().sizeDelta.x) - 10f, 0f, delegate ()
            {
                Log("Get Public Avatars button (Avatar Menu) clicked!", true);

                ApiAvatar currentAvatar = listListObj.avatarPedestal.field_Internal_ApiAvatar_0;
                System.Collections.Generic.List<ApiAvatar> avatars = Sub.GetPublicAvatars(currentAvatar.authorId);

                MelonModLogger.Log("Got " + avatars.Count.ToString() + " Public Avatars for user " + currentAvatar.authorName);

                listObj.transform.Find("Button").GetComponentInChildren<Text>().text = "Public Avatars for user " + currentAvatar.authorName;
                listObj.SetActive(true);
                listListObj.field_Private_Dictionary_2_String_ApiAvatar_0.Clear();

                string[] arr = (from avatar in avatars select avatar.id).ToArray();
                foreach (ApiAvatar avatar in avatars) listListObj.field_Private_Dictionary_2_String_ApiAvatar_0.Add(avatar.id, avatar);
                listListObj.specificListIds = arr;
                listListObj.Method_Protected_Abstract_Virtual_New_Void_Int32_0(0);
            }, false, "/Avatar/Favorite Button", 300f);

            Sub.CreateAvatarButton("Random Public Avatar", -(baseButton.GetComponent<RectTransform>().sizeDelta.x) - 10f, 80f, delegate ()
            {
                Log("Random Public Avatar button clicked!", true);

                Sub.SwitchPedestalToRandomPublicAvatar();
            }, false, "/Avatar/Favorite Button", 300f);

            Button SocialAvatarButton = new Button(); // Placehold
            GameObject SocialAvatarObj;

            (SocialAvatarButton, SocialAvatarObj) = Sub.CreateSocialButton("Get Public Avatars", 215f, 19f, delegate ()
            {
                Log("Get Public Avatars button (Social Menu) clicked!", true);

                PageUserInfo userInfo = GameObject.Find("Screens").transform.Find("UserInfo").transform.GetComponentInChildren<PageUserInfo>();
                System.Collections.Generic.List<ApiAvatar> avatars = Sub.GetPublicAvatars(userInfo.user.id);

                MelonModLogger.Log("Got " + avatars.Count.ToString() + " Public Avatars for user " + userInfo.user.displayName);

                if (avatars.Count == 0)
                {
                    Sub.DoErrorPopup("Public Avatars for " + userInfo.user.displayName, "No public avatars were found for " + userInfo.user.displayName);
                    return;
                }

                listObj.transform.Find("Button").GetComponentInChildren<Text>().text = "Public Avatars for user " + userInfo.user.displayName;
                listObj.SetActive(true);
                listListObj.field_Private_Dictionary_2_String_ApiAvatar_0.Clear();

                string[] arr = (from avatar in avatars select avatar.id).ToArray();
                foreach (ApiAvatar avatar in avatars) listListObj.field_Private_Dictionary_2_String_ApiAvatar_0.Add(avatar.id, avatar);
                listListObj.specificListIds = arr;
                listListObj.Method_Protected_Abstract_Virtual_New_Void_Int32_0(0);

                // Switch to avatar page
                VRCUiManager.prop_VRCUiManager_0.Method_Public_VRCUiPage_VRCUiPage_0(Sub.GetUiObject("/Avatar").gameObject.GetComponentInChildren<VRCUiPage>());
            }, xsize: 200f);

            Sub.CreateSlider(Sub.GetUiObject("/Avatar/Stats Button").transform.parent, baseButton.transform.localPosition.x, baseButton.transform.localPosition.y + (80f*3), baseButton.GetComponent<RectTransform>().sizeDelta.x, 0f, 360f, delegate(float val)
            {
                Log("Slider value changed to " + val.ToString(), true);
                try
                {
                    Transform tf = GameObject.Find("Screens").transform.Find("Avatar").GetComponent<PageAvatar>().avatar.field_Private_GameObject_0.transform;

                    tf.localRotation = Quaternion.Euler(new Vector3(0, tf.localRotation.eulerAngles.y + (val - lastVal), 0));

                    lastVal = val;
                    skipRotationFix = true;
                } 
                catch (Exception e)
                {
                    WarnLog("Failed to change avatar preview rotation.", true);
                }
            }, 180f);

            emmCheck = true;
        }

        public override void OnUpdate()
        {
            // emmVRC compatability
            if (emmCheck)
            {
                ApiWorld currentRoom = RoomManagerBase.field_Internal_Static_ApiWorld_0;
                if (currentRoom != null && currentRoom.id != "")
                {
                    emmCheck = false;
                    return;
                }

                Button[] buttons = Sub.GetUiObject("/Avatar/").GetComponentsInChildren<Button>();

                foreach (Button button in buttons)
                {
                    if (button.name == "Favorite Button(Clone)" && (button.GetComponentInChildren<Text>().text.Contains("Favorite") || button.GetComponentInChildren<Text>().text.Contains("Unfavorite")) && button.gameObject.transform.localPosition == new Vector3(-561f, -227f, -2f))
                    {
                        if (button.gameObject.transform.localPosition == new Vector3(-561f, -227f, -2f))
                        {
                            WarnLog("Found non-compatable emmVRC install, doing compatability changes.");

                            Sub.ChangeObjectPositionBy(button.gameObject, -(baseButton.GetComponent<RectTransform>().sizeDelta.x) - 10f, 0f);
                            button.gameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, button.gameObject.transform.GetComponent<RectTransform>().sizeDelta.y);
                        }
                        emmCheck = false;
                        return;
                    }
                }
            }

            try
            {
                GameObject tf = GameObject.Find("Screens").transform.Find("Avatar").GetComponent<PageAvatar>().avatar.field_Private_GameObject_0;

                if (tf.transform.localPosition.y < 70f || tf.transform.localPosition.y > 75) // Avatar Preview Position Adjustment
                {
                    tf.transform.localPosition = new Vector3(tf.transform.localPosition.x, 70f, tf.transform.localPosition.z);
                }

                if (!skipRotationFix) // Avatar Preview Anti-Rotation
                {
                    tf.transform.rotation = Quaternion.Euler(new Vector3(tf.transform.rotation.eulerAngles.x, lastRotation, tf.transform.rotation.eulerAngles.z));
                }
                else skipRotationFix = false;

                lastRotation = tf.transform.rotation.eulerAngles.y;
            }
            catch
            {

            }
        }
    }
}