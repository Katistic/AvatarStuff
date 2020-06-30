using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Security;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json;

using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnhollowerRuntimeLib;

using VRC.UI;
using VRC.Core;

namespace AvatarStuff
{
    class Sub
    {

        // Helper methods
        public static void ChangeObjectPositionBy(GameObject obj, float x, float y)
        {
            obj.transform.localPosition += new Vector3(x, y);
        }

        public static void SwitchPedestalToAvatar(ApiAvatar avatar, bool confirm = false)
        {
            PageAvatar menu = GameObject.Find("Screens").transform.Find("Avatar").GetComponent<PageAvatar>();
            UnityEngine.Object.Destroy(menu.avatar.field_Private_GameObject_0);
            menu.avatar.Method_Public_Void_ApiAvatar_0(avatar);

            if (confirm) menu.ChangeToSelectedAvatar(); //menu.avatar.field_Internal_ApiAvatar_0 = avatar;
        }

        public static GameObject GetUiObject(string path, bool full = false)
        {
            // Main.Log(full ? path : "/UserInterface/MenuContent/Screens" + path, true);
            return GameObject.Find(full ? path : "/UserInterface/MenuContent/Screens" + path);
        }

        // Ui Creators
        public static GameObject CreateObjectFrom(GameObject obj, bool removeButtonIndicators = false, Transform parent = null)
        {
            obj = UnityEngine.Object.Instantiate<GameObject>(obj, parent ?? obj.transform.parent, true);

            if (removeButtonIndicators)
            {
                foreach (Component image in obj.GetComponent<Button>().GetComponentsInChildren(Il2CppType.Of<Image>()))
                {
                    if (image.name.StartsWith("Icon_")) UnityEngine.Object.DestroyImmediate(image);
                }
            }

            return obj;
        }

        public static GameObject CreateAvatarButton(string text, float x, float y, System.Action action, bool absolute = false, string baseButton = "/Avatar/Favorite Button", float xsize = 0f, float ysize = 0f)
        {
            GameObject publicAvatarButtonObject = CreateObjectFrom(GetUiObject(baseButton), true);
            Button publicAvatarButton = publicAvatarButtonObject.GetComponent<Button>();

            if (!absolute) publicAvatarButtonObject.transform.localPosition =
                    new Vector3(publicAvatarButtonObject.transform.localPosition.x + x,
                    publicAvatarButtonObject.transform.localPosition.y + y);
            else publicAvatarButtonObject.transform.localPosition = new Vector3(x, y);

            Vector2 sd = publicAvatarButtonObject.GetComponent<RectTransform>().sizeDelta;
            publicAvatarButtonObject.GetComponent<RectTransform>().sizeDelta = new Vector2((xsize != 0) ? xsize : sd.x, (ysize != 0) ? ysize : sd.y);

            publicAvatarButtonObject.GetComponentInChildren<Text>().text = text;
            publicAvatarButton.onClick.RemoveAllListeners();
            publicAvatarButton.onClick.AddListener(action);

            return publicAvatarButtonObject;
        }

        public static Slider CreateSlider(Transform parent, float x, float y, float width, float min, float max, Action<float> action, float value = 0f)
        {
            Slider newSlider = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<UIDeviceSelector>().FirstOrDefault(e => e.name == "AudioDevicePanel")?.transform.Find("VolumeSlider")?.GetComponent<Slider>(), parent);

            newSlider.minValue = min;
            newSlider.maxValue = max;
            newSlider.value = value;
            newSlider.transform.localPosition = new Vector3(x, y);
            newSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(width, newSlider.GetComponent<RectTransform>().sizeDelta.y);

            newSlider.onValueChanged = new Slider.SliderEvent();
            if (action != null) newSlider.onValueChanged.AddListener((Action<float>)(v => action(v)));

            return newSlider;
        }

        public static (Button, GameObject) CreateSocialButton(string text, float x, float y, System.Action action, bool absolute = false, string baseButton = "/UserInfo/User Panel/Moderator/Actions/Warn", float xsize = 0f, float ysize = 0f)
        {
            Transform transform = GetUiObject("/UserInfo").gameObject.transform;
            GameObject obj = CreateObjectFrom(GetUiObject(baseButton));
            Button butt = obj.GetComponent<Button>();

            if (!absolute) obj.transform.localPosition += new Vector3(x, y);
            else obj.transform.localPosition.Set(x, y, obj.transform.localPosition.z);

            Vector2 sd = obj.GetComponent<RectTransform>().sizeDelta;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2((xsize != 0) ? xsize : sd.x, (ysize != 0) ? ysize : sd.y);
            butt.GetComponent<RectTransform>().sizeDelta = new Vector2((xsize != 0) ? xsize : sd.x, (ysize != 0) ? ysize : sd.y);

            obj.GetComponentInChildren<Text>().text = text;
            butt.onClick = new Button.ButtonClickedEvent();
            butt.onClick.AddListener(action);

            butt.gameObject.SetActive(true);
            butt.enabled = true;
            butt.GetComponentInChildren<Image>().color = Color.green;

            butt.transform.SetParent(transform);

            return (butt, obj);
        }

        // 'Do' methods
        public static void DoErrorPopup(string title, string content)
        {
            MelonModLogger.LogWarning(content);
            VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0.Method_Public_Void_String_String_Single_3(title, content, 20f);
        }

        // 'Get' methods
        public static List<ApiAvatar> GetPublicAvatars(string uId)
        {
            if (uId == null || !uId.StartsWith("usr_")) return null;

            WebRequest request = WebRequest.Create("https://api.vrchat.cloud/api/1/avatars?apiKey=" + API.ApiKey + "&userId=" + uId + "&order=descending");
            ServicePointManager.ServerCertificateValidationCallback = (System.Object s, X509Certificate c, X509Chain cc, SslPolicyErrors ssl) => true;
            WebResponse response = request.GetResponse();

            string result = "";
            using (Stream rs = response.GetResponseStream())
            {

                using (StreamReader sr = new StreamReader(rs))
                {
                    result = sr.ReadToEnd();
                }
            }
            response.Dispose();

            List<Dictionary<string, dynamic>> list = JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(result);
            List<ApiAvatar> avatarList = new List<ApiAvatar>();

            List<string> listOThings = new List<string>()
            {
                "name", "id", "description", "authorId", "authorName", "imageUrl", "thumbnailImage", "assetUrl"
            };

            foreach (Dictionary<string, dynamic> dAvatar in list)
            {
                ApiAvatar avatar = new ApiAvatar();
                foreach (string key in dAvatar.Keys)
                {
                    if (listOThings.Contains(key))
                    {
                        avatar.WriteField(key, dAvatar[key]);
                    }
                }

                avatarList.Add(avatar);
            }
            return avatarList;
        }

        public static void SwitchPedestalToRandomPublicAvatar()
        {
            string id;

            WebRequest request = WebRequest.Create("http://vrcavatars.tk/public/random");
            ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate c, X509Chain cc, SslPolicyErrors ssl) => true;
            WebResponse response = request.GetResponse();

            using (StreamReader rs = new StreamReader(response.GetResponseStream())) id = Regex.Replace(rs.ReadToEnd(), @"\n", "");

            ApiAvatar avatar = new ApiAvatar();
            avatar.id = id;

            avatar.Get(new Action<ApiContainer>(delegate (ApiContainer container)
            {
                if (avatar.releaseStatus != "public")
                {
                    SwitchPedestalToRandomPublicAvatar();
                    return;
                }

                SwitchPedestalToAvatar(avatar);
            }));
        }
    }
}
