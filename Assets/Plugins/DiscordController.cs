using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;

public class DiscordController : MonoBehaviour
{

    public Discord.Discord discord;
    
    
    // Start is called before the first frame update
    void Start()
    {


         

        discord = new Discord.Discord(992821256674082846, (System.UInt64)Discord.CreateFlags.Default);
        var activityManager = discord.GetActivityManager();
        var activity = new Discord.Activity {
           Assets = {
                LargeImage = ("icone")
           }
        };
        activityManager.UpdateActivity(activity, (res) =>
        {if (res == Discord.Result.Ok)
                Debug.Log("Discord status set");
            else

                Debug.LogError("discord status failed!");
        });
    }

    // Update is called once per frame
    void Update()
    {
        discord.RunCallbacks();
    }



  
}
