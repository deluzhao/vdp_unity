using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using UnityEngine.Networking;
using System.Text;

public class TakeInput : MonoBehaviour
{
    public InputField input;
    public Text solution;
    public Button nextButton;
    public Dropdown image_type;
    public Text score;


    // for file writing
    private string path;
    private string randomId;
    private string fileString;
    private List<string> image_set;
    private int image_set_index;

    // keeping track of mouse positions periodically
    private List<string> mouse_positions;
    private List<Dictionary<string, float>> mouse_durations;
    private float wait_timer;
    private string dict_string;

    // Google Form being written to
    [SerializeField]
    private string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSdDOCMaD_mbHAZJLYjyMEcLxZ3eYGIaZ_bmdq7aTx9m_PzO2A/formResponse";

    private Dictionary<string, List<int>> observations; // for the list of ints, -1 means unknown, 0 means false, 1 means true
    private string observation; // user input observation

    private int index_dropdown;

    void Start()
    {
        image_type = image_type.GetComponent<Dropdown>();
        observations = new Dictionary<string, List<int>>();


        nextButton.onClick.AddListener(RecordFileOnClick);

        image_set = new List<string>() { "Basic", "Bench", "Dog on Couch", "Plates", "Fruit Pile", "Is Dog", "Kitchen", "Driving Direction", "Tv Is On" };
        randomId = "";
        string characters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
        for (int i = 0; i < 10; i++)
        {
            randomId += characters[Random.Range(0, characters.Length)];
        }
        fileString = "User ID: " + randomId + "\n\n";
        path = Application.persistentDataPath + "/" + randomId + "_data" + ".txt";
        Debug.Log(path);
        nextButton.interactable = false;
        mouse_positions = new List<string>() { "", "", "", "", "", "", "", "", "" };
        wait_timer = 0;
        image_set_index = 0;
        CreateDictionaries();
    }


    void Update()
    {
        wait_timer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("pressed enter.");
            AddInputIntoDict();
        }
        if (Input.GetMouseButtonDown(1))
        {
            /*
            if (mouse_positions[image_set_index].Length >= 1)
            {
                mouse_positions[image_set_index] = mouse_positions[image_set_index].Remove(mouse_positions[image_set_index].Length - 1, 1);
            }
            if (mouse_positions[image_set_index] != "")
            {
                mouse_positions[image_set_index] += "\n";
            }
            */
            CheckMousePosition();
            wait_timer = 0;
        }
        if (Input.GetMouseButton(1))
        {
            if (wait_timer > 0.05)
            {
                CheckMousePosition();
                wait_timer = 0;
            }
        }
    }

    void CreateDictionaries()
    {
        Dictionary<string, float> starting_dict1 = new Dictionary<string, float>();
        Dictionary<string, float> starting_dict2 = new Dictionary<string, float>();
        Dictionary<string, float> starting_dict3 = new Dictionary<string, float>();
        Dictionary<string, float> starting_dict4 = new Dictionary<string, float>();
        Dictionary<string, float> starting_dict5 = new Dictionary<string, float>();
        Dictionary<string, float> starting_dict6 = new Dictionary<string, float>();
        Dictionary<string, float> starting_dict7 = new Dictionary<string, float>();
        Dictionary<string, float> starting_dict8 = new Dictionary<string, float>();
        Dictionary<string, float> starting_dict9 = new Dictionary<string, float>();
        mouse_durations = new List<Dictionary<string, float>>() { starting_dict1, starting_dict2, starting_dict3, starting_dict4, starting_dict5, starting_dict6, starting_dict7, starting_dict8, starting_dict9 };
    }
    // grabs the mouse position in world and writes it to a string assigned to the specific puzzle being solved
    void CheckMousePosition()
    {
        Vector3 v3 = Input.mousePosition;
        v3.z = 0;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        if (image_set_index < 9)
        {
            //mouse_positions[image_set_index] += "(" + v3.x + "," + v3.y + "):";
            bool create_key = true;
            foreach (string key in mouse_durations[image_set_index].Keys)
            {
                string[] x_and_y = key.Split(',');
                Vector3 existing;
                if (x_and_y[0].Substring(0, 1) == "F" || x_and_y[0].Substring(0, 1) == "D")
                {
                    existing = new Vector3(float.Parse(x_and_y[0].Substring(1)), float.Parse(x_and_y[1]), 0);
                }
                else
                {
                    existing = new Vector3(float.Parse(x_and_y[0]), float.Parse(x_and_y[1]), 0);
                }
                float dist = Vector3.Distance(v3, existing);
                if (dist <= 500)
                {
                    mouse_durations[image_set_index][key] += Time.deltaTime;
                    create_key = false;
                    break;
                }
            }
            if (create_key)
            {
                string new_key = v3.x + "," + v3.y;
                if (Input.GetMouseButtonDown(1))
                {
                    if (mouse_durations[image_set_index].Count == 0)
                    {
                        new_key = "F" + new_key;
                    }
                    else
                    {
                        new_key = "D" + new_key;
                    }
                }
                mouse_durations[image_set_index].Add(new_key, Time.deltaTime);
            }
        }
    }

    void AddInputIntoDict()
    {
        nextButton.interactable = false;
        bool obs_type = true;
        observation = input.text.Trim().ToLower();
        if (observation.Length >= 3 && observation.Substring(0, 3) == "not")
        {
            observation = observation.Substring(3).Trim();
            obs_type = false;
        }
        index_dropdown = image_type.value;
        Debug.Log("index chosen is: " + index_dropdown + ", observation is: " + observation);
        if (observation == "")
        {
            Debug.Log("not a valid output");
            return;
        }
        if (observation == "skip")
        {
            observations.Add(observation, new List<int>() { 1, 1, 0, 0 });
            input.text = "";
            bool test = CheckForSolution();
            return;
        }
        observation = observation.Replace(" ", "_");
        if (observations.ContainsKey(observation))
        {
            if (obs_type)
            {
                observations[observation][index_dropdown] = 1;
            }
            else
            {
                observations[observation][index_dropdown] = 0;
            }
        }
        else
        {
            observations.Add(observation, new List<int>() { -1, -1, -1, -1 });
            if (obs_type)
            {
                observations[observation][index_dropdown] = 1;
            }
            else
            {
                observations[observation][index_dropdown] = 0;
            }
        }
        input.text = "";
        bool solution_found = CheckForSolution();
    }

    bool CheckForSolution()
    {
        Dictionary<string, bool> trueForPos = new Dictionary<string, bool>();
        Dictionary<string, bool> falseForNeg = new Dictionary<string, bool>();
        foreach (string key in observations.Keys)
        {
            List<int> values = observations[key];
            Debug.Log(key + ": " + values[0] + values[1] + values[2] + values[3]);
            if (values[0] == 1 && values[1] == 1)
            {
                trueForPos.Add(key, true);
            }
            else
            {
                trueForPos.Add(key, false);
            }
            if (values[2] == 0 && values[3] == 0)
            {
                falseForNeg.Add(key, true);
            }
            else
            {
                falseForNeg.Add(key, false);
            }
        }

        foreach (string key in observations.Keys)
        {
            if (trueForPos[key] && falseForNeg[key])
            {
                solution.text = "Solution: " + key;
                Debug.Log("solution found for: " + key);
                if (image_set_index <= 8)
                {
                    nextButton.interactable = true;
                }
                input.enabled = false;
                return true;
            }
        }

        if (observations.Count >= 2)
        {
            foreach (string key in observations.Keys)
            {
                foreach (string key2 in observations.Keys)
                {
                    List<int> values = observations[key];
                    List<int> values2 = observations[key2];
                    bool allTrue = true; // operator for if two observations are true for both positive images but are not true for both negative images
                    if (values[2] == 0 && values2[3] == 0)
                    {
                        allTrue = false;
                    }
                    if (values[3] == 0 && values2[2] == 0)
                    {
                        allTrue = false;
                    }
                    if (trueForPos[key] && trueForPos[key2] && !allTrue)
                    {
                        solution.text = "Solution: " + key + " and " + key2;
                        Debug.Log("solution found for: " + key + " in conjunction with " + key2);
                        if (image_set_index <= 8)
                        {
                            nextButton.interactable = true;
                        }
                        input.enabled = false;
                        return true;
                    }
                }
            }
        }

        // reverse functionality
        Dictionary<string, bool> falseForPos = new Dictionary<string, bool>();
        Dictionary<string, bool> trueForNeg = new Dictionary<string, bool>();
        foreach (string key in observations.Keys)
        {
            List<int> values = observations[key];
            Debug.Log(key + ": " + values[0] + values[1] + values[2] + values[3]);
            if (values[0] == 0 && values[1] == 0)
            {
                falseForPos.Add(key, true);
            }
            else
            {
                falseForPos.Add(key, false);
            }
            if (values[2] == 1 && values[3] == 1)
            {
                trueForNeg.Add(key, true);
            }
            else
            {
                trueForNeg.Add(key, false);
            }
        }

        foreach (string key in observations.Keys)
        {
            if (falseForPos[key] && trueForNeg[key])
            {
                solution.text = "Solution: " + key;
                Debug.Log("solution found for: " + key);
                if (image_set_index <= 8)
                {
                    nextButton.interactable = true;
                }
                input.enabled = false;
                return true;
            }
        }

        if (observations.Count >= 2)
        {
            foreach (string key in observations.Keys)
            {
                foreach (string key2 in observations.Keys)
                {
                    List<int> values = observations[key];
                    List<int> values2 = observations[key2];
                    bool allTrue = true;
                    if (values[1] == 0 && values2[0] == 0)
                    {
                        allTrue = false;
                    }
                    if (values[0] == 0 && values2[1] == 0)
                    {
                        allTrue = false;
                    }
                    if (trueForNeg[key] && trueForNeg[key2] && !allTrue)
                    {
                        solution.text = "Solution: " + key + " and " + key2;
                        Debug.Log("solution found for: " + key + " in conjunction with " + key2);
                        if (image_set_index <= 8)
                        {
                            nextButton.interactable = true;
                        }
                        input.enabled = false;
                        return true;
                    }
                }
            }
        }
        solution.text = "Solution: none found";
        nextButton.interactable = false;
        input.enabled = true;
        return false;
    }

    void RecordFileOnClick()
    {
        nextButton.interactable = false;
        input.enabled = true;
        fileString += "IMAGESET " + image_set[image_set_index] + "\n";
        foreach (string key in observations.Keys)
        {
            fileString += key + ": ";
            foreach (int image_value in observations[key])
            {
                if (image_value == 1)
                {
                    fileString += "T ";
                }
                else if (image_value == 0)
                {
                    fileString += "F ";
                }
                else
                {
                    fileString += "U ";
                }
            }
            fileString += "\n";
        }
        fileString += "\n";
        image_set_index++;
        observations = new Dictionary<string, List<int>>();
        solution.text = "Solution: none found";
        if (image_set_index == 8)
        {
            nextButton.GetComponentInChildren<Text>().text = "Finish";
        }
        if (image_set_index == 9)
        {
            SendToForm();
            input.enabled = false;
            nextButton.GetComponentInChildren<Text>().text = "Your ID: " + randomId;
        }
    }

    void SendToForm()
    {
        StartCoroutine(Post(fileString, randomId));
    }

    IEnumerator Post(string fileStr, string id)
    {
        List<string> dict_positions = new List<string>();
        //path = Application.persistentDataPath + "/temp.txt";
        for (int i = 0; i < 9; i++)
        {
            dict_string = image_set[i] + ": ";
            foreach (string key in mouse_durations[i].Keys)
            {
                dict_string += "(" + key + "," + mouse_durations[i][key] + ")";
            }
            dict_positions.Add(dict_string);
        }
        //File.WriteAllText(path, dict_string);
        WWWForm form = new WWWForm();
        form.AddField("entry.293234153", fileStr);
        /*
        for (int i = 0; i < 9; i++)
        {
            if (mouse_positions[i].Length >= 1)
            {
                mouse_positions[i] = mouse_positions[i].Remove(mouse_positions[i].Length - 1, 1);
            }
            if (mouse_positions[i].Length > 50000)
            {
                mouse_positions[i] = mouse_positions[i].Substring(0, 50000);
            }
        }
        */
        form.AddField("entry.745416448", dict_positions[0]);
        form.AddField("entry.97934866", dict_positions[1]);
        form.AddField("entry.1617567216", dict_positions[2]);
        form.AddField("entry.1333477229", dict_positions[3]);
        form.AddField("entry.1718651490", dict_positions[4]);
        form.AddField("entry.1822252734", dict_positions[5]);
        form.AddField("entry.415110004", dict_positions[6]);
        form.AddField("entry.773575948", dict_positions[7]);
        form.AddField("entry.629382885", dict_positions[8]);
        form.AddField("entry.16015621", score.text);
        UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Completed successfully!");
        }
    }
}
