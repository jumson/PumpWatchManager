using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System;
using System.IO;

public class myCanvas : MonoBehaviour
{

	public TextMeshProUGUI serverAddress;
	// = GetComponent<InputField>();
	public int pingTime;
	public TextMeshProUGUI consoleText;
	//checking the internet and pinging code from: https://answers.unity.com/questions/1246460/how-do-i-check-connection-to-internet-in-android.html
	//getting html from: https://answers.unity.com/questions/567497/how-to-100-check-internet-availability.html
	//wide chars: 46 then new line
	private const float waitingTime = 5.0f;
	private string address;
	private Ping ping;
	private float pingStartTime;
	List<string> lines = new List<string> ();
	//this holds array of strings to scroll like a console

	void Start ()
	{
		lines = new List<string> () { " ", " ", " ", "Welcome to the Pump Watcher App!" };
		updateConsole ();
		CheckInternet ();
		address = "http://blynk.metamunson.com:7000/WYSIWYG!"; //my default for now, to test
		serverAddress.text = address;
		string HtmlText = GetHtmlFromUri (address);

		if (HtmlText == "") {
			updateConsole ("google not found though...");
		} else if (!HtmlText.Contains ("schema.org/WebPage")) {
			//Redirecting since the beginning of googles html contains that 
			//phrase and it was not found
			updateConsole ("not google...");
			updateConsole (HtmlText);
		} else {
			updateConsole (HtmlText);
		}
	}

	void CheckInternet ()
	{
		bool internetPossiblyAvailable;
		switch (Application.internetReachability) {
		case NetworkReachability.ReachableViaLocalAreaNetwork:
			internetPossiblyAvailable = true;
			break;
		case NetworkReachability.ReachableViaCarrierDataNetwork:
             //internetPossiblyAvailable = allowCarrierDataNetwork;
			internetPossiblyAvailable = true;
			break;
		default:
			internetPossiblyAvailable = false;
			break;
		}
		if (!internetPossiblyAvailable) {
			updateConsole ("Internet NOT Detected");
			return;
		}
		updateConsole ("Internet Detected");

	}

	public void doPing ()
	{
		//address = "blynk,metamunson.com"; //serverAddress.text.ToString ();

		updateConsole ("checking: " + address);	
		string HtmlText = GetHtmlFromUri (address);
		updateConsole (HtmlText);	
		//ping = new Ping (address);
		//pingStartTime = Time.time;
		
	}

	public void onServerIn ()
	{
		address = serverAddress.text.ToString ();
		updateConsole ("Server Address: " + address);
	}

	public void Update ()
	{
		if (ping != null) {
			bool stopCheck = true;
			if (ping.isDone)
				updateConsole ("pinged at " + (Time.time - pingStartTime).ToString () + " seconds");
			else if (Time.time - pingStartTime < waitingTime) {
				stopCheck = false;
			} else if ((Time.time - pingStartTime > waitingTime)) {
				updateConsole ("ping timed out after: " + waitingTime.ToString () + " seconds");
			} else
				updateConsole ("ping failed");
			if (stopCheck)
				ping = null;
		}

	}

	public void OnExit ()
	{
		updateConsole ("leaving App");
		Application.Quit ();
	}

	void updateConsole (string data = "")
	{
		//45 chars in a line for portrait
		if (data != "") {
			if (data.Length > 45 && data.Length < 90) {
				lines.Add (data.Substring (0, 44));
				lines.Add (data.Substring (45));
			} else {
				lines.Add (data);
			}
			if (data.Length > 89) {
				lines.Add ("more than 90 chars cut off");
			}
		}
		consoleText.SetText (lines [lines.Count - 4] + "\n" + lines [lines.Count - 3] + "\n" + lines [lines.Count - 2] + "\n" + lines [lines.Count - 1]); 
		//print the 4 lines

	}

	public string GetHtmlFromUri (string resource)
	{
		string html = string.Empty;
		HttpWebRequest req = (HttpWebRequest)WebRequest.Create (resource);
		try {
			using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse ()) {
				bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
				if (isSuccess) {
					using (StreamReader reader = new StreamReader (resp.GetResponseStream ())) {
						//We are limiting the array to 80 so we don't have
						//to parse the entire html document feel free to 
						//adjust (probably stay under 300)
						char[] cs = new char[80];
						reader.Read (cs, 0, cs.Length);
						foreach (char ch in cs) {
							html += ch;
						}
					}
				}
			}
		} catch {
			return "";
		}
		return html;
	}

}
