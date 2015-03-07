using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

public class Transaction<T> where T : class
{
    T response;
    public Transaction()
    {
        
    }

    public IEnumerator HttpGetRequest(string requestUrl)
    {
        WWW www = new WWW(requestUrl);
        yield return www;
        response = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(www.text);
    }

    public T GetResponse()
    {
        if (null != response)
        {
            return response;
        }
        else
        {
            Debug.LogError("No Response Data Present!");
            return default(T);
        }
    }

}
