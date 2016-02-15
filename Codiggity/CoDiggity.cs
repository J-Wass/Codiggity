using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Media;
using System.Threading;
using System.Diagnostics;

namespace CoDiggity
{
    public partial class Codiggity : Form
    {
        List<String> nouns = File.ReadAllLines("lib/nouns.txt").ToList<String>();
        List<String> verbs = File.ReadAllLines("lib/verbs.txt").ToList<String>();
        List<String> adjectives = File.ReadAllLines("lib/adjectives.txt").ToList<String>();
        List<String> adverbs = File.ReadAllLines("lib/adverbs.txt").ToList<String>();
        List<String> prepositions = File.ReadAllLines("lib/prepositions.txt").ToList<String>();
        List<String> pronouns = File.ReadAllLines("lib/pronouns.txt").ToList<String>();
        string block = "";
        bool wipe = false;

        Random rand = new Random();

        public Codiggity()
        {
            InitializeComponent();
        }
        private string getRhyme(string word)
        {
            string url = "http://rhymebrain.com/talk?function=getRhymes&word=" + word; //+ word
            var proxy = WebRequest.Create(url);
            var response = proxy.GetResponse();
            var stream = response.GetResponseStream();

            StreamReader sReader = new StreamReader(stream);

            string json = sReader.ReadToEnd();

            Word[] items = JsonConvert.DeserializeObject<Word[]>(json);
            int length = items.Count();
            int randomWord = rand.Next(1, length);
            int i = 0;
            while (items[randomWord].score < 300 && i < 1000)
            {
                randomWord = rand.Next(1, length);
                i++;
            }
            return items[randomWord].word;
        }
        private string getPartOfSpeach(string word)
        {
            string url = "http://dictionary.reference.com/browse/" + word + "?s=t";
            var proxy = WebRequest.Create(url);
            var response = proxy.GetResponse();
            var stream = response.GetResponseStream();
            StreamReader sReader = new StreamReader(stream);
            string json = sReader.ReadToEnd();
            string partOfSpeach = Regex.Split(((Regex.Split(json, "class=\"dbox-pg\">")[1])), "</span>")[0];
            return partOfSpeach;

        }
        private string findSynonym(string word)
        {
            string url = "http://www.thesaurus.com/browse/" + word + "?s=t";
            var proxy = WebRequest.Create(url);
            var response = proxy.GetResponse();
            var stream = response.GetResponseStream();
            StreamReader sReader = new StreamReader(stream);
            string json = sReader.ReadToEnd();
            string partOfSpeach = Regex.Split(((Regex.Split(json, "<div class=\"relevancy-list\" style=\"margin-left: 0px;\">")[1])), "<span class=\"text\">")[1];
            //			string partOfSpeach = Regex.Split(Regex.Split (((Regex.Split (json, "<div class=\"relevancy-list\" style=\"margin-left: 0px;\">") [1])), "<span class=\"text\">") [1], "</span>")[0];
            return partOfSpeach;
        }
        private string generateSentence(string rhymeWord)
        {
            int nounRand = rand.Next(nouns.Count() - 1);
            int nounRand2 = rand.Next(nouns.Count() - 1);
            int verbRand = rand.Next(verbs.Count() - 1);
            int adjectiveRand = rand.Next(adjectives.Count() - 1);
            int adverbRand = rand.Next(adverbs.Count() - 1);
            int prepRand = rand.Next(prepositions.Count() - 1);
            int pronounRand = rand.Next(pronouns.Count() - 1);

            string noun = nouns.ToArray()[nounRand];
            string adjective = "";
            string article = "a";
            if (adjectiveRand % 4 >=  1)
            {
                adjective = adjectives.ToArray()[adjectiveRand];
                char first = adjective.ToCharArray()[0];
                if (first.Equals('a') || first.Equals('e') || first.Equals('i') || first.Equals('o') || first.Equals('u'))
                {
                    article += 'n';
                }
            }
            else
            {
                char first = noun.ToCharArray()[0];
                if (first.Equals('a') || first.Equals('e') || first.Equals('i') || first.Equals('o') || first.Equals('u'))
                {
                    article += 'n';
                }
            }
            if (pronounRand % 2 == 0)
            {
                article = "the";
            }
            string preposition = "";
            if (nounRand % 2 == 0)
            {
                preposition = prepositions.ToArray()[prepRand] + " ";
            }
            string subject = "";
            if (rand.Next(1, 10) < 8)
            {
                subject = pronouns.ToArray()[pronounRand];
            }
            return (subject + " " + verbs.ToArray()[verbRand] + " " + preposition
                + article + " " + adjectives.ToArray()[adjectiveRand] + " " + rhymeWord);
        }
        private void bGo_Click(object sender, EventArgs e)
        {
            tOutput.Clear();
            SpeechSynthesizer synth = new SpeechSynthesizer();
            if (checkBox1.Checked)
            {
                synth.SelectVoice("Microsoft Server Speech Text to Speech Voice (en-GB, Hazel)");
            }
            else
            {
                synth.SelectVoice("Microsoft Server Speech Text to Speech Voice (en-US, Helen)");
            }
            if (block.StartsWith("Let me show"))
            {
                tTheme.Text = "demo";
                goto speak;
            }
            /*String voices = "";
            foreach(InstalledVoice v in synth.GetInstalledVoices())
            {
                voices += v.VoiceInfo.Name + "\n";
            }
            MessageBox.Show(voices);
            //			SpeechSynthesizer synth = new SpeechSynthesizer();
            //			synth.SelectVoice("Microsoft Zira Desktop");
            //			synth.Volume = 100;
            //			synth.Rate = 1; http://www.dictionaryapi.com/api/v1/references/collegiate/xml/tTheme.Text?key=9307aec8-5ca3-476c-8a1b-0100a646c429*/
            string rhymeWord = getRhyme(tTheme.Text);
            while (getPartOfSpeach(rhymeWord) != "noun")
            {
                rhymeWord = getRhyme(tTheme.Text);
            }
            int nounRand = rand.Next(nouns.Count() - 1);
            string noun = nouns.ToArray()[nounRand];
            //string syn1 = findSyn(noun)[(new Random).Next(findSyn(noun).Count())];
            string firstParagraph = generateSentence(tTheme.Text) + Environment.NewLine + generateSentence(getRhyme(tTheme.Text)) + Environment.NewLine + generateSentence(noun)  + Environment.NewLine + generateSentence(getRhyme(noun));
            nounRand = rand.Next(nouns.Count() - 1);
            noun = nouns.ToArray()[nounRand];
            nounRand = rand.Next(nouns.Count() - 1);
            string noun1 = nouns.ToArray()[nounRand];
            string secondParagraph = "\n\r\n\r" + generateSentence(noun1) + Environment.NewLine + generateSentence(getRhyme(noun1)) + Environment.NewLine + generateSentence(noun) +   Environment.NewLine + generateSentence(getRhyme(noun));
            nounRand = rand.Next(nouns.Count() - 1);
            noun = nouns.ToArray()[nounRand];
            nounRand = rand.Next(nouns.Count() - 1);
            noun1 = nouns.ToArray()[nounRand];
            string thirdParagraph = "\n\r\n\r" + generateSentence(noun1) + Environment.NewLine + generateSentence(getRhyme(noun1)) + Environment.NewLine + generateSentence(noun) + Environment.NewLine +  generateSentence(getRhyme(noun));
            nounRand = rand.Next(nouns.Count() - 1);
            noun = nouns.ToArray()[nounRand];
            string finalTwo = "\n\r\n\r" + generateSentence(noun) + Environment.NewLine + generateSentence(getRhyme(noun));
        //MessageBox.Show(firstParagraph + secondParagraph + thirdParagraph + finalTwo);
            synth.Rate = 1;
            //tOutput.AppendText(synth.Voice.Name + Environment.NewLine);
            block = firstParagraph + secondParagraph + thirdParagraph + finalTwo;
            speak:
            synth.SetOutputToWaveFile("lib\\" + tTheme.Text + ".wav");
            tOutput.AppendText(block);
            synth.Speak(block);

            SoundPlayer simpleSound = new SoundPlayer(Directory.GetCurrentDirectory() + "\\lib\\" + tTheme.Text + ".wav");

            synth.SetOutputToDefaultAudioDevice();
            string[] startPhrase = { "giddy up", "buckle up", "wiggity wiggity wack", "are you ready?", "listen up" };
            int selectPhase = rand.Next(0,startPhrase.Count());
            synth.Speak(startPhrase[selectPhase]);
            Thread.Sleep(200);

            string[] songs = {"SecondSample.m4a", "ThirdSampleMP4.m4a", "FourthSampleMP4.m4a" };
            int selectSong = rand.Next(0, songs.Count());
            var p2 = new System.Windows.Media.MediaPlayer();
            string backround = Directory.GetCurrentDirectory() + "\\lib\\" + songs[selectSong];
            p2.Volume = 0.2;
            p2.Open(new System.Uri(backround));
            DateTime first = DateTime.Now;
            p2.Play();
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            while ((DateTime.Now - first).Seconds < 6.5)
            {
                Random gen = new Random();
                progressBar1.Value = gen.Next(10,30);
                progressBar2.Value = gen.Next(20,40);
            }
            simpleSound.Play();
            first = DateTime.Now;
            while((DateTime.Now-first).Seconds < 34)
            {
                Random gen = new Random();
                progressBar1.Value = gen.Next(20,40);
                progressBar2.Value = gen.Next(30,60);
                progressBar3.Value = gen.Next(60, 100);

            }
            p2.Volume = 0.1;
            while ((DateTime.Now - first).Seconds < 2)
            {
                Random gen = new Random();
                progressBar1.Value = gen.Next(20, 40);
                progressBar2.Value = gen.Next(30, 60);
                progressBar3.Value = gen.Next(60, 100);

            }
            if (wipe)
            {
                block = "";
                wipe = false;
            }
            first = DateTime.Now;
            simpleSound.Stop();
            p2.Stop();
            button1.Enabled = true;
            button2.Enabled = true;
            progressBar1.Value = 0;
            progressBar2.Value = 0;
            progressBar3.Value = 0;
            /*
            var p1 = new System.Windows.Media.MediaPlayer();
            string speech = Directory.GetCurrentDirectory() + "\\lib\\" + tTheme.Text + ".wav";
            MessageBox.Show(speech);
            p1.Volume = 100;
            p1.Open(new System.Uri(speech));
            p1.Play();*/

        }

        private void CodeDiggity_Load(object sender, EventArgs e)
        {
           // this.WindowState = FormWindowState.Maximized;
        }

        private string[] findSyn(string word)
        {
            string url = "http://www.thesaurus.com/browse/" + word + "?s=t";
            var proxy = WebRequest.Create(url);
            var response = proxy.GetResponse();
            var stream = response.GetResponseStream();
            StreamReader sReader = new StreamReader(stream);
            string json = sReader.ReadToEnd();
            string[] partOfSpeach = Regex.Split(json, "<span class=\"text\">");
            string[] synonyms = new string[partOfSpeach.Count() - 1];
            for (int i = 1; i < partOfSpeach.Count(); i++)
            {
                string synonym = Regex.Split(partOfSpeach[i], "</span>")[0];
                synonyms[i - 1] = synonym;
            }
            return synonyms;
        }

        private void logo_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Rap saved to " + Directory.GetCurrentDirectory() + "\\lib\\" + tTheme.Text + ".wav");
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\lib\\";
            openFileDialog1.ShowDialog();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\lib\\" + tTheme.Text + ".txt", block);
            Process.Start("notepad.exe", Directory.GetCurrentDirectory() + "\\lib\\" + tTheme.Text + ".txt");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            block = "Let me show you how we do it in Rutgers son " + Environment.NewLine +
                "You'll be back to hello world by the time we are done " + Environment.NewLine +
                "Codiggity is fire, our program is so fresh" + Environment.NewLine +
                "But don't look at our code, it's a bit of a mess" + Environment.NewLine +
                "We're here at hack B U, with a rapping machine" + Environment.NewLine +
                "Let's get on with the demo, there's so much to be seen.";
            wipe = true;
            bGo.PerformClick();
        }

        private void progressBar2_Click(object sender, EventArgs e)
        {

        }
    }


    public class Word
    {
        public string word { get; set; }
        public string freq { get; set; }
        public int score { get; set; }
        public string flags { get; set; }
        public string syllables { get; set; }
    }

}
