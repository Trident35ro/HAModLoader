using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Globalization;
using UnityEngine.Advertisements;
using UnityEngine;

public class Loader : MonoBehaviour
{
    private enum ad_state
    {
        REQUEST_AGAIN = 0,
        requesting = 1,
        ready_to_show = 2
    }

    public enum ad_context
    {
        gameplay_reward = 0,
        after_few_levelups = 1,
        on_breeder = 2
    }

    public static Loader Instance;

    public GameObject type_creatureModel;

    public GameObject type_limb;

    public GameObject type_limb_dummy;

    public GameObject type_snapPoint_;

    public UnityEngine.Object[] textureList;

    public UnityEngine.Object[] eyeList;

    public UnityEngine.Object[] mouthList;

    public Sprite[] creatureSprites;

    public string[] temp_static_names;

    public List<string> temp_static_names_list = new List<string>();

    public string[] animationNames;

    public int[] maxFrames;

    private float scaleMod = 0.7f;

    [HideInInspector]
    public string session_animal = "";

    [HideInInspector]
    public bool screen_resized;

    [HideInInspector]
    public Dictionary<Vector3, respawn> to_respawn_slotA = new Dictionary<Vector3, respawn>();

    [HideInInspector]
    public Dictionary<Vector3, respawn> to_respawn_slotB = new Dictionary<Vector3, respawn>();

    [HideInInspector]
    public Dictionary<Vector3, respawn> to_respawn_slotC = new Dictionary<Vector3, respawn>();

    private const string InterstitialPlacement = "Interstitial_Android";
    private const string RewardedPlacement = "Rewarded_Android";
    public string unityGameId = "YOUR_UNITY_GAME_ID";
    public bool unityTestMode = false;

    private bool wanting_to_view_reward;

    [HideInInspector]
    public bool reward_video_complete;

    private ad_state interstitial_ad_state = ad_state.REQUEST_AGAIN;

    private ad_state reward_ad_state = ad_state.REQUEST_AGAIN;

    private int request_interstitial_timer;

    private int request_reward_timer;

    public GameObject prefab_model_replacer;

    public int ADS_num_rebreeds;

    [HideInInspector]
    public int last_ad_skipped;

    private bool show_joined = true;

    public static int n_stats = 8;

    [HideInInspector]
    public bool is_muted;

    private Dictionary<string, GameObject> hybrids_in_existence = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            string[] array = temp_static_names;
            foreach (string item in array)
            {
                temp_static_names_list.Add(item);
            }
        }
        else
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }

    private void Start()
    {
        if (this == Instance)
        {
            if (!Advertisement.isInitialized)
            {
                Advertisement.Initialize(unityGameId, unityTestMode, new UnityAdsInitListener(this));
            }
            TryRequestInterstitial();
            TryRequestRewardAd();

            loadAllTextures();
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        }
        else
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }

    private bool PlacementReady(string placementId)
    {
        if (placementId == RewardedPlacement) return reward_ad_state == ad_state.ready_to_show;
        if (placementId == InterstitialPlacement) return interstitial_ad_state == ad_state.ready_to_show;
        return false;
    }

    private void TryRequestRewardAd()
    {
        if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified || Application.systemLanguage == SystemLanguage.ChineseTraditional) return;
        if (reward_ad_state != ad_state.REQUEST_AGAIN) return;

        reward_ad_state = ad_state.requesting;
        try
        {
            Advertisement.Load(RewardedPlacement, new UnityAdsLoadListener(this, RewardedPlacement));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Load Rewarded failed: " + ex.Message);
            reward_ad_state = ad_state.REQUEST_AGAIN;
            reward_video_complete = true;
        }
    }

    private void TryRequestInterstitial()
    {
        if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified || Application.systemLanguage == SystemLanguage.ChineseTraditional) return;
        if (interstitial_ad_state != ad_state.REQUEST_AGAIN) return;

        interstitial_ad_state = ad_state.requesting;
        try
        {
            Advertisement.Load(InterstitialPlacement, new UnityAdsLoadListener(this, InterstitialPlacement));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Load Interstitial failed: " + ex.Message);
            interstitial_ad_state = ad_state.REQUEST_AGAIN;
        }
    }

    private void do_show_interstitial()
    {
        if (!PlacementReady(InterstitialPlacement))
        {
            Debug.Log("Interstitial ad not ready.");
            return;
        }

        try
        {
            interstitial_ad_state = ad_state.REQUEST_AGAIN;
            Advertisement.Show(InterstitialPlacement, new UnityAdsShowListener(this, InterstitialPlacement));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Show Interstitial failed: " + ex.Message);
        }
    }

    private void do_show_reward()
    {
        reward_video_complete = false;
        if (!PlacementReady(RewardedPlacement))
        {
            Debug.Log("Rewarded ad not ready.");
            PopupControl.Instance.ShowMessage("Ad not available right now.");
            wanting_to_view_reward = false;
            return;
        }

        try
        {
            reward_ad_state = ad_state.REQUEST_AGAIN;
            Advertisement.Show(RewardedPlacement, new UnityAdsShowListener(this, RewardedPlacement));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Show Rewarded failed: " + ex.Message);
            wanting_to_view_reward = false;
        }
    }

    private class UnityAdsInitListener : IUnityAdsInitializationListener
    {
        private Loader parent;
        public UnityAdsInitListener(Loader p) { parent = p; }
        public void OnInitializationComplete()
        {
            Debug.Log("[Loader] Unity Ads initialized");
            parent.TryRequestInterstitial();
            parent.TryRequestRewardAd();
        }
        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"[Loader] Unity Ads init failed: {error} - {message}");
        }
    }

    private class UnityAdsLoadListener : IUnityAdsLoadListener
    {
        private Loader parent;
        private string placement;
        public UnityAdsLoadListener(Loader p, string placementId) { parent = p; placement = placementId; }
        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId == Loader.RewardedPlacement) parent.reward_ad_state = ad_state.ready_to_show;
            if (placementId == Loader.InterstitialPlacement) parent.interstitial_ad_state = ad_state.ready_to_show;

            if (placementId == Loader.RewardedPlacement && parent.wanting_to_view_reward)
            {
                parent.wanting_to_view_reward = false;
                parent.do_show_reward();
            }
        }
        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogWarning($"[Loader] Load failed {placementId}: {error} - {message}");
            if (placementId == Loader.RewardedPlacement) parent.reward_ad_state = ad_state.REQUEST_AGAIN;
            if (placementId == Loader.InterstitialPlacement) parent.interstitial_ad_state = ad_state.REQUEST_AGAIN;
        }
    }

    private class UnityAdsShowListener : IUnityAdsShowListener
    {
        private Loader parent;
        private string placement;
        public UnityAdsShowListener(Loader p, string placementId) { parent = p; placement = placementId; }
        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogWarning($"[Loader] Show failed {placementId}: {error} - {message}");
            if (placementId == Loader.RewardedPlacement) parent.reward_ad_state = ad_state.REQUEST_AGAIN;
            if (placementId == Loader.InterstitialPlacement) parent.interstitial_ad_state = ad_state.REQUEST_AGAIN;
        }
        public void OnUnityAdsShowStart(string placementId) { PopupControl.Instance.HideAll(); }
        public void OnUnityAdsShowClick(string placementId) { }
        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (placementId == Loader.RewardedPlacement)
            {
                if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
                {
                    parent.reward_video_complete = true;
                    PopupControl.Instance.ShowRewardCompletePopup();
                }
                else
                {
                    parent.reward_video_complete = false;
                    GameController.Instance.LOCK_LEVEL_SCREEN = false;
                    PopupControl.Instance.ShowMessage("You must finish the video to recieve the reward!");
                }
                parent.reward_ad_state = ad_state.REQUEST_AGAIN;
            }
            else if (placementId == Loader.InterstitialPlacement)
            {
                parent.interstitial_ad_state = ad_state.REQUEST_AGAIN;
            }
        }
    }

    private void OnAdFinished(string placementId, object rawShowResult)
    {
        // for when watching ads will be reintoduced
    }

    public void SHOW_AD(ad_context context)
    {
        switch (context)
        {
            case ad_context.on_breeder:
                if (PlayerData.Instance.GetGlobalInt("no_ads") != 1)
                {
                    ADS_num_rebreeds++;
                    if (ADS_num_rebreeds >= 2 && PlacementReady(InterstitialPlacement))
                    {
                        do_show_interstitial();
                        ADS_num_rebreeds = 0;
                    }
                }
                break;
            case ad_context.after_few_levelups:
                if (PlayerData.Instance.GetGlobalInt("no_ads") == 1)
                {
                    break;
                }
                if (last_ad_skipped > 0)
                {
                    if (Application.isEditor)
                    {
                        PopupControl.Instance.ShowMessage("ADVERTISEMENT");
                    }
                    else if (PlacementReady(InterstitialPlacement))
                    {
                        do_show_interstitial();
                    }
                    last_ad_skipped--;
                }
                else
                {
                    PopupControl.Instance.ShowRewardAskPopup(true);
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        request_interstitial_timer++;
        if (request_interstitial_timer == 60)
        {
            TryRequestInterstitial();
            request_interstitial_timer = 0;
        }
        request_reward_timer++;
        if (request_reward_timer == 60)
        {
            TryRequestRewardAd();
            request_reward_timer = 0;
        }

        if (wanting_to_view_reward && reward_ad_state == ad_state.ready_to_show && PlacementReady(RewardedPlacement))
        {
            wanting_to_view_reward = false;
            do_show_reward();
        }
    }

    public void RecycleCreature(GameObject creature)
    {
        UnityEngine.Object.Destroy(creature);
    }

    public GameObject load_player_creature(string morphStr, int slot)
    {
        int slotInt = PlayerData.Instance.GetSlotInt("n_morphed_creatures", PlayerData.grouping_t.general, slot);
        List<string> list = new List<string>();
        for (int i = 0; i < slotInt; i++)
        {
            int slotInt2 = PlayerData.Instance.GetSlotInt("morph" + i, PlayerData.grouping_t.general, slot);
            list.Add(temp_static_names[slotInt2]);
        }
        return GetHybrid(list);
    }

    public void skipBreedScreen()
    {
        StartCoroutine(SKIP_BREED());
    }

    private IEnumerator SKIP_BREED()
    {
        yield return new WaitForSeconds(0.1f);
        GameController.Instance.CreateNibsLists();
        NewBreedControl.Instance.SetElevatorAtFinishedPos();
        NewAudioControl.Instance.play_gameplay_music();
        inventory_ctr.Instance.unique_id_iterator = PlayerData.Instance.GetSlotInt("unique_id_iterator", PlayerData.grouping_t.general);
        GameObject gameObject = load_player_creature("skipBreed", PlayerData.Instance.SLOT);
        NewBreedControl.Instance.result = gameObject;
        gameObject.transform.position = NewBreedControl.Instance.campos_result.position;
        gameObject.transform.parent = NewBreedControl.Instance.campos_result;
        gameObject.transform.localRotation = Quaternion.identity;
        StartCoroutine(NewBreedControl.Instance.transition_to_gameplay(0.1f, false, true));
        NewBreedControl.Instance.text_result_name.text = PlayerData.Instance.GetSlotString("creatureName", PlayerData.grouping_t.general);
        NewBreedControl.Instance.num_rebreeds = PlayerData.Instance.GetSlotInt("numRebreeds", PlayerData.grouping_t.general);
        NewGameControl.Instance.create_mutant_particle(NewBreedControl.Instance.num_rebreeds, GameController.Instance.player.GetComponent<creatureScript>());
        inventory_ctr.Instance.load_inventory();
        GameController.Instance.playerLevel = PlayerData.Instance.GetSlotInt("playerLevel", PlayerData.grouping_t.general);
        GameController.Instance.text_playerLevel.text = "Level " + GameController.Instance.playerLevel;
        GameController.Instance.skillPointsSpendable = PlayerData.Instance.GetSlotInt("skillPointsSpendable", PlayerData.grouping_t.general);
        GameController.Instance.text_skillPoints.text = GameController.Instance.skillPointsSpendable + " points";
        GameController.Instance.currentEXP = PlayerData.Instance.GetSlotInt("currentEXP", PlayerData.grouping_t.general);
        GameController.Instance.temp_nextLevelExp_ = PlayerData.Instance.GetSlotInt("temp_nextLevelExp", PlayerData.grouping_t.general);
        int[] array = new int[n_stats];
        for (int i = 0; i < n_stats; i++)
        {
            int num = (array[i] = PlayerData.Instance.GetSlotInt("stat" + i, PlayerData.grouping_t.general));
            for (int j = 0; j < num % 6; j++)
            {
                GameController.Instance.create_nib(i);
            }
            if (num != 0)
            {
                GameController.Instance.set_lvl_name(i, num);
            }
        }
        GameController.Instance.player_stats = array;
        GameController.Instance.apply_player_effects();
        int slotInt = PlayerData.Instance.GetSlotInt("player_chunk_x", PlayerData.grouping_t.playerpos);
        int slotInt2 = PlayerData.Instance.GetSlotInt("player_chunk_z", PlayerData.grouping_t.playerpos);
        int slotInt3 = PlayerData.Instance.GetSlotInt("player_inner_x", PlayerData.grouping_t.playerpos);
        int slotInt4 = PlayerData.Instance.GetSlotInt("player_inner_z", PlayerData.grouping_t.playerpos);
        if (slotInt != 0 || slotInt2 != 0 || slotInt3 != 0 || slotInt4 != 0)
        {
            if (slotInt == 999 && slotInt2 == 999 && slotInt3 == 999 && slotInt4 == 999)
            {
                GameController.Instance.player.transform.position = NewBreedControl.Instance.campos_result.transform.position + Vector3.up * 2f;
            }
            else
            {
                GameController.Instance.player.transform.position = new Vector3(slotInt * 10 + slotInt3, 0.5f, slotInt2 * 10 + slotInt4) + new Vector3(0.5f, 0f, 0.5f);
            }
        }
        snap_cam();
        string text = PlayerData.Instance.GetSlotString("player_zone", PlayerData.grouping_t.playerpos);
        if (text == "")
        {
            text = "overworld";
        }
        NewBiomeControl.Instance.player_zone = text;
        NewBiomeControl.Instance.player_zone_type = PlayerData.Instance.GetSlotString("player_zone_type", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.zone_origin_chunkX = PlayerData.Instance.GetSlotInt("zone_origin_chunk_x", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.zone_origin_chunkZ = PlayerData.Instance.GetSlotInt("zone_origin_chunk_z", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.zone_origin_innerX = PlayerData.Instance.GetSlotInt("zone_origin_inner_x", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.zone_origin_innerZ = PlayerData.Instance.GetSlotInt("zone_origin_inner_z", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.zone_rotation = PlayerData.Instance.GetSlotInt("zone_rotation", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.shack_entrance_chunkX = PlayerData.Instance.GetSlotInt("shack_entrance_chunk_x", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.shack_entrance_chunkZ = PlayerData.Instance.GetSlotInt("shack_entrance_chunk_z", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.shack_entrance_innerX = PlayerData.Instance.GetSlotInt("shack_entrance_inner_x", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.shack_entrance_innerZ = PlayerData.Instance.GetSlotInt("shack_entrance_inner_z", PlayerData.grouping_t.playerpos);
        NewBiomeControl.Instance.set_zone_models();
        NewBiomeControl.Instance.TerrainChanged(true);
        NewBiomeControl.Instance.update_chunk_display();
        perk_controller.Instance.load_saved_perks();
        show_joined = false;
        yield return new WaitForSeconds(0.1f);
    }

    public void snap_cam()
    {
        GameController.Instance.mainCamera.transform.position = GameController.Instance.player.transform.position + new Vector3(5f, 15f, -5f) * 0.1f;
        GameController.Instance.mainCamera.transform.LookAt(GameController.Instance.player.transform.position);
    }

    public void load_companions()
    {
        GameController.Instance.n_hatchlings_ = PlayerData.Instance.GetSlotInt("num_hatchlings", PlayerData.grouping_t.general);
        if (GameController.Instance.n_hatchlings_ < 0)
        {
            GameController.Instance.n_hatchlings_ = 0;
        }
        for (int i = 0; i < GameController.Instance.n_hatchlings_; i++)
        {
            int slotInt = PlayerData.Instance.GetSlotInt("friendly_creatureA" + i, PlayerData.grouping_t.general);
            int slotInt2 = PlayerData.Instance.GetSlotInt("friendly_creatureB" + i, PlayerData.grouping_t.general);
            int slotInt3 = PlayerData.Instance.GetSlotInt("friendly_lvl" + i, PlayerData.grouping_t.general);
            GameObject obj = Shop_positioner.Instance.spawn_companion(GameController.Instance.player.transform.position + Vector3.right * 1.8f * (i + 1), slotInt, slotInt2, slotInt3, i);
            obj.GetComponent<creatureScript>().friendly_exp = PlayerData.Instance.GetSlotInt("friendly_exp" + i, PlayerData.grouping_t.general);
            obj.GetComponent<creatureScript>().friendly_nextLevelExp = PlayerData.Instance.GetSlotInt("friendly_nextLevelExp" + i, PlayerData.grouping_t.general);
        }
    }

    public string GET_PATH(string filename)
    {
        bool flag = false;
        if (Application.platform == RuntimePlatform.Android)
        {
            flag = true;
        }
        if (flag)
        {
            Debug.Log("a");
            WWW wWW = new WWW(Path.Combine(Application.streamingAssetsPath, filename));
            while (!wWW.isDone)
            {
            }
            string text = Application.persistentDataPath + "/db";
            File.WriteAllBytes(text, wWW.bytes);
            return text;
        }
        return Path.Combine(Application.streamingAssetsPath, filename);
    }

    private void loadAllTextures()
    {
        textureList = Resources.LoadAll("Textures/Skins/", typeof(Texture2D));
        eyeList = Resources.LoadAll("Textures/Eyes/", typeof(Texture2D));
        mouthList = Resources.LoadAll("Textures/Mouths/", typeof(Texture2D));
    }

    public void setCubeColor(float[] hsv_get, GameObject g, Color theColor)
    {
        HsvColor hsvColor = HSVUtil.ConvertRgbToHsv(theColor);
        float num = 100f;
        float num2 = hsvColor.normalizedH + hsv_get[0] / num;
        float value = hsvColor.normalizedS + hsv_get[1] / num * 3f;
        float value2 = hsvColor.normalizedV + hsv_get[2] / num * 4f;
        float num3 = num2 % 1f;
        value = Mathf.Clamp(value, 0f, 1f);
        value2 = Mathf.Clamp(value2, 0f, 1f);
        Color color = HSVUtil.ConvertHsvToRgb(num3 * 360f, value, value2, 1f);
        g.GetComponent<limb_scr>().visual.GetComponent<Renderer>().material.color = color;
        if (g.GetComponent<limb_scr>().dummy != null)
        {
            g.GetComponent<limb_scr>().dummy.vis.GetComponent<Renderer>().material.color = color;
        }
    }

    public void setTexture(string str, GameObject g)
    {
        if (str == "None")
        {
            g.GetComponent<limb_scr>().visual.GetComponent<Renderer>().material.mainTexture = null;
            return;
        }
        if (g.GetComponent<limb_scr>().isDecorative)
        {
            if (g.GetComponent<limb_scr>().compName == "eyes")
            {
                for (int i = 0; i < eyeList.Length; i++)
                {
                    if (eyeList[i].name == str)
                    {
                        g.transform.parent.gameObject.GetComponent<creatureModel>().eyeObject_ = g.GetComponent<limb_scr>();
                        g.GetComponent<limb_scr>().visualPlane.GetComponent<Renderer>().material.mainTexture = eyeList[i] as Texture2D;
                        break;
                    }
                }
            }
            if (!(g.GetComponent<limb_scr>().compName == "mouth"))
            {
                return;
            }
            for (int j = 0; j < mouthList.Length; j++)
            {
                if (mouthList[j].name == str)
                {
                    g.GetComponent<limb_scr>().visualPlane.GetComponent<Renderer>().material.mainTexture = mouthList[j] as Texture2D;
                    g.transform.parent.gameObject.GetComponent<creatureModel>().mouthObject_open = mouthList[j] as Texture2D;
                    g.transform.parent.gameObject.GetComponent<creatureModel>().mouthObject_blink = mouthList[j + 1] as Texture2D;
                    break;
                }
            }
            return;
        }
        for (int k = 0; k < textureList.Length; k++)
        {
            if (textureList[k].name == str)
            {
                g.GetComponent<limb_scr>().visual.GetComponent<Renderer>().material.mainTexture = textureList[k] as Texture2D;
                break;
            }
        }
    }

    public GameObject GetHybrid(List<string> parents)
    {
        string text = "";
        foreach (string parent in parents)
        {
            text = text + parent + "+";
        }
        if (hybrids_in_existence.ContainsKey(text))
        {
            if (hybrids_in_existence[text] != null)
            {
                GameObject obj = CloneCreature(hybrids_in_existence[text]);
                obj.name = "clone";
                return obj;
            }
            hybrids_in_existence.Remove(text);
        }
        List<GameObject> list = new List<GameObject>();
        foreach (string parent2 in parents)
        {
            GameObject singleAnimal = GetSingleAnimal(parent2);
            list.Add(singleAnimal);
        }
        GameObject gameObject = GetComponent<morpher>().morphCreatures_(list, false, 0);
        foreach (GameObject item in list)
        {
            UnityEngine.Object.Destroy(item);
        }
        hybrids_in_existence.Add(text, gameObject);
        return gameObject;
    }

    public GameObject GetSingleAnimal(string animal)
    {
        return Physically_Load_Creature(GET_PATH("creatures/" + animal + ".tbc"), false, animal);
    }

    private GameObject Physically_Load_Creature(string path, bool justHead, string myname)
    {
        GameObject[] array = null;
        GameObject gameObject = UnityEngine.Object.Instantiate(type_creatureModel);
        gameObject.GetComponent<creatureModel>().Init();
        gameObject.GetComponent<creatureModel>().creatures_that_made_me.Add(myname);
        gameObject.name = "Creature-" + myname;
        gameObject.GetComponent<creatureModel>().myName = myname;
        using (StreamReader streamReader = new StreamReader(path))
        {
            gameObject.GetComponent<creatureModel>().height = float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
            gameObject.GetComponent<creatureModel>().height_set = true;
            Color white = Color.white;
            white.r = float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
            white.g = float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
            white.b = float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
            gameObject.GetComponent<creatureModel>().myCol = white;
            streamReader.ReadLine();
            int.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
            gameObject.GetComponent<creatureModel>().seed = float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
            gameObject.GetComponent<creatureModel>().namePrefix = streamReader.ReadLine();
            gameObject.GetComponent<creatureModel>().nameSuffix = streamReader.ReadLine();
            int num = int.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
            streamReader.ReadLine();
            array = new GameObject[num];
            gameObject.GetComponent<creatureModel>().children_ = array;
            for (int i = 0; i < num; i++)
            {
                array[i] = UnityEngine.Object.Instantiate(type_limb);
                array[i].GetComponent<limb_scr>().Init();
                array[i].GetComponent<limb_scr>().setupFrames(false);
            }
            bool flag = false;
            GameObject head_obj = null;
            gameObject.GetComponent<creatureModel>().body_obj = array[0];
            for (int j = 0; j < num; j++)
            {
                array[j].transform.parent = gameObject.transform;
                limb_scr component = array[j].GetComponent<limb_scr>();
                GameObject visual = component.visual;
                component.compName = streamReader.ReadLine();
                if (j == 0)
                {
                    head_obj = component.gameObject;
                }
                if (!flag && component.compName == "head")
                {
                    gameObject.GetComponent<creatureModel>().head_obj = component.gameObject;
                    flag = true;
                }
                component.setDecorative(bool.Parse(streamReader.ReadLine()));
                component.symmetrical = bool.Parse(streamReader.ReadLine());
                array[j].GetComponent<limb_scr>().invertSymmAnm = bool.Parse(streamReader.ReadLine());
                component.inherit = bool.Parse(streamReader.ReadLine());
                visual.transform.localScale = new Vector3(float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture), float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture), float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture));
                visual.transform.localPosition = new Vector3(float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture), float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture), float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture));
                int num2 = int.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
                if (num2 != -1)
                {
                    component.parent_ = array[num2];
                    component.create_snap(component.parent_.GetComponent<limb_scr>().visual.transform);
                }
                component.children_ = new List<GameObject>();
                int num3 = int.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
                for (int k = 0; k < num3; k++)
                {
                    component.children_.Add(array[int.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture)]);
                }
                for (int l = 0; l < 4; l++)
                {
                    component.hsv_values[l] = float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
                }
                component.textureName = streamReader.ReadLine();
                setCubeColor(component.hsv_values, array[j], gameObject.GetComponent<creatureModel>().myCol);
                setTexture(component.textureName, array[j]);
                int num4 = int.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
                for (int m = 0; m < num4; m++)
                {
                    int num5 = int.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture);
                    for (int n = 0; n < num5; n++)
                    {
                        Quaternion quaternion = new Quaternion(float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture), float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture), float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture), float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture));
                        Vector3 vector = new Vector3(float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture), float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture), float.Parse(streamReader.ReadLine(), CultureInfo.InvariantCulture));
                        if (n < component.frames_rotations[m].Length)
                        {
                            component.frames_rotations[m][n] = quaternion;
                            component.frames_snapPositions[m][n] = vector;
                        }
                    }
                }
                streamReader.ReadLine();
            }
            if (!flag)
            {
                gameObject.GetComponent<creatureModel>().head_obj = head_obj;
            }
            for (int num6 = 0; num6 < num; num6++)
            {
                if (array[num6].GetComponent<limb_scr>().symmetrical)
                {
                    array[num6].GetComponent<limb_scr>().chain_makeDummy(true);
                }
                array[num6].transform.rotation = array[num6].GetComponent<limb_scr>().frames_rotations[0][0];
                if (array[num6].GetComponent<limb_scr>().parent_ != null)
                {
                    array[num6].GetComponent<limb_scr>().set_snap_local(array[num6].GetComponent<limb_scr>().frames_snapPositions[0][0]);
                }
            }
        }
        array[0].transform.localPosition = Vector3.zero;
        return gameObject;
    }

    public GameObject CloneCreature(GameObject A)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate(A);
        gameObject.SetActive(true);
        creatureModel component = A.GetComponent<creatureModel>();
        creatureModel component2 = gameObject.GetComponent<creatureModel>();
        for (int i = 0; i < component.children_.Length; i++)
        {
            limb_scr component3 = component.children_[i].GetComponent<limb_scr>();
            limb_scr component4 = component2.children_[i].GetComponent<limb_scr>();
            component4.frames_snapPositions = new List<Vector3[]>();
            foreach (Vector3[] frames_snapPosition in component3.frames_snapPositions)
            {
                Vector3[] array = new Vector3[frames_snapPosition.Length];
                for (int j = 0; j < frames_snapPosition.Length; j++)
                {
                    array[j] = frames_snapPosition[j];
                }
                component4.frames_snapPositions.Add(array);
            }
            component4.frames_rotations = new List<Quaternion[]>();
            foreach (Quaternion[] frames_rotation in component3.frames_rotations)
            {
                Quaternion[] array2 = new Quaternion[frames_rotation.Length];
                for (int k = 0; k < frames_rotation.Length; k++)
                {
                    array2[k] = frames_rotation[k];
                }
                component4.frames_rotations.Add(array2);
            }
            component4.setDecorative();
            component4.Init();
        }
        component2.Init();
        component2.CloneAdjustHeight(component);
        return gameObject;
    }

    public void ShowRewardAd()
    {
        if (Application.isEditor)
        {
            PopupControl.Instance.HideAll();
            PopupControl.Instance.ShowRewardCompletePopup();
            return;
        }

        if (reward_ad_state == ad_state.ready_to_show && PlacementReady(RewardedPlacement))
        {
            do_show_reward();
        }
        else
        {
            wanting_to_view_reward = true;
            PopupControl.Instance.ShowConnecting("Loading Reward Ad");
            TryRequestRewardAd();
        }
    }
}
