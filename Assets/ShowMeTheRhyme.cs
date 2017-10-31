using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ShowMeTheRhyme : MonoBehaviour 
{
	public InputField inputField;
	public GameObject noHangle;
	public GameObject notFind;
	public Text resultText;
	public GameObject dataLoad;
	public Text loadText;
	public GameObject errorNtf;
	public Text errorText;

	bool isLoaded = false;
	List<string> m_listLine = new List<string>();

	void Start () 
	{
		isLoaded = false;
		m_listLine.Clear ();

		loadText.text = "데이터 로드 준비";
		dataLoad.SetActive (true);

		List<string> fileNames = new List<string> ();
		fileNames.Add ("data_0");
		fileNames.Add ("data_1");
		fileNames.Add ("data_2");

		StartCoroutine (LoadTextFile (fileNames));
	}

	IEnumerator LoadTextFile(List<string> fileNames)
	{
		int index = 0;
		string line = "";

		for (int i = 0; i < fileNames.Count; i++) 
		{
			index = 0;
			line = "";
			//StreamReader sr = new StreamReader (Application.dataPath + "/Resources/" + fileNames[i]);
			TextAsset data = Resources.Load(fileNames[i], typeof(TextAsset)) as TextAsset;
			StringReader sr = new StringReader(data.text);
			if (sr == null) {
				// 오류. 재시작.
				errorNtf.SetActive (true);
				errorText.text = "(1001)";
				yield break;
			} else {
				line = sr.ReadLine ();
				while (line != null) {
					m_listLine.Add (line);
					line = sr.ReadLine ();
					index += 1;
					if (index == 1 || index % 100 == 0) {
						//Debug.Log (string.Format("# Loading Lines:{0}",index));
						loadText.text = string.Format ("데이터 로드 중...[{0}]\n({1}/{2})",index,i+1,fileNames.Count);
						yield return null;
					}
				}
				sr.Close ();
			}
		}

		//Debug.Log (string.Format ("# Load File Line Count:{0}",m_listLine.Count));
		loadText.text = string.Format("데이터 로드 완료({0})",m_listLine.Count);
		yield return new WaitForSeconds (0.5f);
		dataLoad.SetActive (false);

		isLoaded = true;
	}

	public void OnClickedSearch()
	{
		if (isLoaded) {
			StartCoroutine (SearchRhyme ());
		}
	}

	IEnumerator SearchRhyme()
	{
		List<int> rhyme = Hangul_parser (inputField.text);

		if (rhyme.Count <= 1) {
			noHangle.SetActive (true);
			yield break;
		}

		bool equal = false;

		List<string> result = new List<string> ();

		// 데이터에서 한줄씩 실행.
		for(int i=0; i<m_listLine.Count; i++)
		{
			// 데이터 한줄을 라임으로 뽑음.
			List<int> temp = Hangul_parser(m_listLine[i]);
			// 근데 라임보다 글자수가 작으면 아웃.
			if (temp.Count < rhyme.Count)
				continue;

			equal = false;

			int rhymeIdx = 0;
			int tempIdx = 0;
			while(true) 
			{
				if (rhyme.Count <= rhymeIdx) {
					break;
				}

				if (rhyme [rhymeIdx] == 0)
				{
					equal = true;
				}

				if (equal == false) 
				{
					if (temp.Count <= tempIdx) {
						break;
					}
					// 라임과 라인 하나씩 비교.
					if (rhyme [rhymeIdx] == temp [tempIdx]) 
					{
						// 맞으면 인덱스 하나씩 증가.
						rhymeIdx++;
						tempIdx++;
					} 
					else 
					{
						// 틀리면 라임 인덱스 다시 앞으로 돌리고 진행.
						rhymeIdx = 0;
						tempIdx++;
					}
				} 
				else 
				{
					int startIdx = tempIdx - (rhyme.Count - 1);
					result.Add(m_listLine [i].Substring (startIdx, rhyme.Count - 1));
					break;
				}
			}

			if (i % 1000 == 0) {
				yield return null;
				Debug.Log (string.Format ("# Searching Line:{0}", i));
			}

			if (result.Count >= 20) {
				// 일단 테스트로 20개.
				break;
			}
		}

		if (result.Count <= 0) {
			notFind.SetActive (true);
		}
		else {
			string resultStr = string.Empty;
			for (int i = 0; i < result.Count; i++) {
				resultStr = string.Format ("{0}\n{1}",resultStr,result[i]);
			}
			resultText.text = resultStr;
		}
	}

	public List<int> Hangul_parser(string typo)
	{
		List<int> rime_start = new List<int> ();
		for (int i = 0; i < typo.Length; i++) 
		{
			char comVal = (char) (typo.ToCharArray()[i] - 0xAC00);
			// 한글일경우 // 초성만 입력 했을 시엔 초성은 무시해서 List에 추가합니다.
			if (comVal >= 0 && comVal <= 11172)
			{
				// 유니코드 표에 맞추어 초성 중성 종성을 분리합니다..
				char uniVal = (char)comVal;
				char jung = (char) ((((uniVal - (uniVal % 28))/28)%21)+0x314f);
				switch(jung)
				{
				case 'ㅏ':
				case 'ㅑ':
				case 'ㅘ':
					rime_start.Add (1);
					//Debug.Log ("1");
					break;

				case 'ㅐ':
				case 'ㅒ':
				case 'ㅔ':
				case 'ㅖ':
				case 'ㅙ':
				case 'ㅚ':
				case 'ㅞ':
					rime_start.Add(2);
					//Debug.Log ("2");
					break;

				case 'ㅓ':
				case 'ㅕ':
				case 'ㅝ':
					rime_start.Add(3);
					//Debug.Log ("3");
					break;

				case 'ㅗ':
				case 'ㅛ':
					rime_start.Add(4);
					//Debug.Log ("4");
					break;

				case 'ㅜ':
				case 'ㅠ':
				case 'ㅡ':
					rime_start.Add(5);
					//Debug.Log ("5");
					break;

				case 'ㅟ':
				case 'ㅢ':
				case 'ㅣ':
					rime_start.Add(6);
					//Debug.Log ("6");
					break;
				}
				// 4519
			}
		}
		rime_start.Add(0);
		return rime_start;
	}
}