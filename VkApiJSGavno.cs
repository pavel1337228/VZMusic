using CefSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace VkMusic
{
    public static class VkJSGavnoApi
    {
        public static async Task PageLoaded(this IChromiumWebBrowserBase browser) 
        {
            await browser.EvaluateScriptAsync("window.vk.audioAdsConfig = null;");
            await browser.EvaluateScriptAsync("document.querySelector('.AudioBlock_triple_stacked_slider').remove();");
            
        }
        public static async Task addMoreAudios(this IChromiumWebBrowserBase browser)
        {
            await browser.EvaluateScriptAsync("document.querySelector('.show_more_wrap').scrollIntoView();");
        }
        private static bool a = false;
        public static async Task<List<Track>> getMoreAudios(this IChromiumWebBrowserBase browser, int count)
        {
            if (!a) { 
                await browser.EvaluateScriptAsync("let a = [...document.querySelectorAll('.audio_item')].map((row) => {var str = eval(row.dataset.audio);var data = {id: str[0],id_user: str[1],title: str[3],autor: str[4],seconds: str[5],};return data;},)");
                a = !a;
            }
            else
                await browser.EvaluateScriptAsync("a = [...document.querySelectorAll('.audio_item')].map((row) => {var str = eval(row.dataset.audio);var data = {id: str[0],id_user: str[1],title: str[3],autor: str[4],seconds: str[5],};return data;},)");

            JavascriptResponse response = await browser.EvaluateScriptAsync(string.Format("JSON.stringify(a.splice({0},a.length));", count));
            if (response.Result != null)
            {
                List<Track> TrackList = JsonConvert.DeserializeObject<List<Track>>(response.Result.ToString());
                return TrackList;
            }
            return new List<Track>();
        }
        public static async Task<List<Track>> getTracks(this IChromiumWebBrowserBase browser)
        {
            JavascriptResponse response = await browser.EvaluateScriptAsync("JSON.stringify([...document.querySelectorAll('.audio_item')].map((row) => {var str = eval(row.dataset.audio);var data = {id: str[0],id_user: str[1],title: str[3],autor: str[4],seconds: str[5],};return data;},));");
            if (response.Result != null)
            {
                List<Track> TrackList = JsonConvert.DeserializeObject<List<Track>>(response.Result.ToString());
                return TrackList;
            }
            return new List<Track>();
        }
        public static async Task<double> thisTrackPosition(this IChromiumWebBrowserBase browser)
        {
            JavascriptResponse response = await browser.EvaluateScriptAsync("JSON.stringify(audio.getPosition());");
            if (response.Result != null)
            {
                var v = double.Parse(response.Result.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                await browser.EvaluateScriptAsync(string.Format("console.log('position track: {0}');", v));
                return v;
            }
            return 0.0;
        }
        public static async Task setVolume(this IChromiumWebBrowserBase browser, double value)
        {
            JavascriptResponse response = await browser.EvaluateScriptAsync(string.Format("audio.volume({0})",value.ToString().Replace(',','.')));
        }
        public static async Task<double> thisTrackDuration(this IChromiumWebBrowserBase browser)
        {
            JavascriptResponse response = await browser.EvaluateScriptAsync("JSON.stringify(audio.duration());");
            if (response.Result != null)
            {
                var v = double.Parse(response.Result.ToString(), System.Globalization.CultureInfo.InvariantCulture); 
                await browser.EvaluateScriptAsync(string.Format("console.log('duration track: {0}');", v));
                return v;
            }
            return 0;
        }
        public static async Task play(this IChromiumWebBrowserBase browser, string id) 
        {
            await browser.EvaluateScriptAsync(string.Format("console.log('audio.play({0})');" + "window.audio.play('{0}');", id));
        }
        public static async Task pause(this IChromiumWebBrowserBase browser)
        {
            await browser.EvaluateScriptAsync(string.Format("console.log('audio.pause()');" + "window.audio.pause();"));
        }
        public static async Task setPosition(this IChromiumWebBrowserBase browser, double value)
        {
            JavascriptResponse response = await browser.EvaluateScriptAsync(string.Format("audio.setPosition({0})", value.ToString().Replace(',', '.')));
        }
        public static async Task pauseStart(this IChromiumWebBrowserBase browser)
        {
            await browser.EvaluateScriptAsync(string.Format("console.log('audio.play()');" + "window.audio.play();"));
        }
        public static async Task writeLine(this IChromiumWebBrowserBase browser, string str)
        {
            await browser.EvaluateScriptAsync(string.Format("console.log('{0}');", str));
        }
    }
}
