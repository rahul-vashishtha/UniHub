using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HtmlAgilityPack;
using Android.Util;
using System.IO;
using UniHub;

namespace UniHub
{
    #region HTML Manager
    static class HtmlManager
    {
        public static Uri ExtractImage(HtmlDocument source)
        {
            if (source.DocumentNode.Descendants("img") != null)
            {
                foreach (HtmlNode link in source.DocumentNode.Descendants("img"))
                {
                    HtmlAttribute att = link.Attributes["src"];
                    return new Uri(att.Value);
                }
            }

            return null;
        }

        public static List<Uri> ExtractImages(HtmlDocument source)
        {
            List<Uri> images = new List<Uri>();

            if (source.DocumentNode.Descendants("img") != null)
            {
                foreach (HtmlNode link in source.DocumentNode.Descendants("img"))
                {
                    if (link.Attributes["src"] != null)
                    {
                        string str = link.Attributes["src"].Value.Replace("-150x150", "");
                        images.Add(new Uri(str));
                    }
                }
            }

            return images;
        }

        //public static PlacementAddition ExtractPlacementImagesData(HtmlDocument source)
        //{
        //    List<Uri> images = new List<Uri>();
        //    List<string> imagesNames = new List<string>();
        //    PlacementAddition pa = new PlacementAddition();

        //    if (source.DocumentNode.Descendants("img") != null)
        //    {
        //        foreach (HtmlNode link in source.DocumentNode.Descendants("img"))
        //        {
        //            images.Add(new Uri(link.Attributes["src"].Value));
        //            imagesNames.Add(link.Attributes["alt"].Value);
        //        }
        //    }

        //    pa.ImageNames = imagesNames;
        //    pa.ImageLinks = images;

        //    return pa;
        //}

        public static List<Uri> ExtractPlacementArticleUrls(HtmlDocument source)
        {
            List<Uri> list = new List<Uri>();

            if (source.DocumentNode.Descendants("a") != null)
            {
                foreach (HtmlNode link in source.DocumentNode.Descendants("a"))
                {
                    list.Add(new Uri(link.Attributes["href"].Value));
                }
            }

            return list;
        }

        public static Uri ExtractFile(HtmlDocument source)
        {
            foreach (HtmlNode link in source.DocumentNode.Descendants("a"))
            {
                string attrib = link.Attributes["href"].Value.ToString().ToLower();

                if (attrib.Contains(".pdf") || attrib.Contains(".doc") || attrib.Contains(".docx") || attrib.Contains(".ppt") || attrib.Contains(".pptx"))
                {
                    return new Uri(link.Attributes["href"].Value);
                }

                return null;
            }

            return null;
        }

        public static string ExtractText(HtmlDocument source)
        {
            string data = string.Empty;

            foreach (HtmlNode link in source.DocumentNode.Descendants("p"))
            {
                data += link.InnerText + "\n";
            }

            return data;
        }
    }
    #endregion

    #region Parse Data
    class ParseData
    {
        // Parse Notice Data
        public static class ReadNotices
        {
            public static void UpdateLastUpdateDate(Context c, string data)
            {
                Settings set = new Settings(c);

                set.LastNoticeUpdate = data;
            }

            public static List<Notices> GetNotices(Context c, XDocument xdoc)
            {
                List<Notices> notices = new List<Notices>();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xdoc.Document.ToString());

                UpdateLastUpdateDate(c, doc.GetElementsByTagName("lastBuildDate")[0].InnerText);

                XmlNodeList nodes = doc.GetElementsByTagName("item");

                foreach (XmlNode nod in nodes)
                {
                    Notices n = new Notices();

                    foreach (XmlNode node in nod.ChildNodes)
                    {
                        if (node.Name == "title")
                        {
                            n.Title = node.InnerText;
                        }

                        if (node.Name == "link")
                        {
                            n.PageLink = new Uri(node.InnerText);
                        }

                        if (node.Name == "pubDate")
                        {
                            n.PublishDate = node.InnerText;
                        }

                        if (node.Name == "content:encoded" || node.Name == "content")
                        {
                            HtmlDocument docHtml = new HtmlDocument();
                            docHtml.LoadHtml("<html>" + node.InnerText + "</html>");

                            Uri url = HtmlManager.ExtractImage(docHtml);

                            if (url != null)
                                n.HasImage = true;
                            else
                                n.HasImage = false;

                            n.ImageLink = url;

                            url = HtmlManager.ExtractFile(docHtml);

                            if (url != null)
                                n.HasDocument = true;
                            else
                                n.HasDocument = false;

                            n.FileLink = url;
                        }
                    }

                    notices.Add(n);
                }

                return notices;
            }
        }

        // Parse Placement Data
        public static class ReadPlacements
        {
            public static void UpdateLastUpdateDate(Context c, string data)
            {
                Settings set = new Settings(c);

                set.LastPlacementUpdate = data;
            }

            public static List<Placements> GetPlacements(Context c, XDocument xdoc)
            {
                List<Placements> placement = new List<Placements>();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xdoc.Document.ToString());

                //UpdateLastUpdateDate(c, doc.GetElementsByTagName("lastBuildDate")[0].InnerText);

                XmlNodeList nodes = doc.GetElementsByTagName("item");

                foreach (XmlNode nod in nodes)
                {
                    Placements n = new Placements();

                    foreach (XmlNode node in nod.ChildNodes)
                    {
                        if (node.Name == "title")
                        {
                            n.Title = node.InnerText;
                        }

                        if (node.Name == "pubDate")
                        {
                            n.PublishDate = node.InnerText;
                        }

                        if (node.Name == "content:encoded" || node.Name == "content")
                        {
                            HtmlDocument docHtml = new HtmlDocument();
                            docHtml.LoadHtml("<html>" + node.InnerText + "</html>");

                            n.Icon = HtmlManager.ExtractImage(docHtml);
                            n.Content = HtmlManager.ExtractText(docHtml);
                            n.Images = HtmlManager.ExtractImages(docHtml);
                        }
                    }

                    placement.Add(n);
                }

                return placement;
            }
        }

        // Parse Question Papers
        public static class ReadQuestionPapers
        {
            public static List<Semester> GetQuestionPapers(Context context)
            {
                List<Semester> semesters = new List<Semester>();
                XmlDocument doc = new XmlDocument();

                using (StreamReader sr = new StreamReader(context.Assets.Open("question_bank.xml")))
                {
                    doc.LoadXml(sr.ReadToEnd());
                }

                XmlNodeList semesterNodes = doc.GetElementsByTagName("Semester");

                foreach (XmlNode semester in semesterNodes)
                {
                    Semester sem = new Semester();

                    sem.SemesterName = semester.Attributes["title"].Value.ToString();
                    sem.Years = new List<Year>();

                    foreach (XmlNode year in semester.ChildNodes)        // ChildNodes are Years here
                    {
                        Year y = new Year();
                        y.YearName = year.Attributes["name"].Value.ToString();
                        y.QuestionPapers = new List<QuestionPaper>();

                        foreach (XmlNode file in year.ChildNodes)        // ChildNodes are Files here
                        {
                            QuestionPaper qp = new QuestionPaper();

                            qp.FileLink = file.Attributes["link"].Value.ToString();
                            qp.FileName = file.InnerText.ToString();

                            y.QuestionPapers.Add(qp);
                        }

                        sem.Years.Add(y);
                    }

                    semesters.Add(sem);
                }

                return semesters;
            }
        }

        // Parse News & Events
        public static class ReadNews
        {
            public static List<NewsEvents> GetNewsAndEvents(Context c, XDocument xdoc)
            {
                List<NewsEvents> news = new List<NewsEvents>();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xdoc.Document.ToString());

                //UpdateLastUpdateDate(c, doc.GetElementsByTagName("lastBuildDate")[0].InnerText);

                XmlNodeList nodes = doc.GetElementsByTagName("item");

                foreach (XmlNode nod in nodes)
                {
                    NewsEvents n = new NewsEvents();

                    foreach (XmlNode node in nod.ChildNodes)
                    {
                        if (node.Name == "title")
                        {
                            n.Title = node.InnerText;
                        }

                        if (node.Name == "pubDate")
                        {
                            n.PublishDate = node.InnerText;
                        }

                        if (node.Name == "content:encoded" || node.Name == "content")
                        {
                            HtmlDocument docHtml = new HtmlDocument();
                            docHtml.LoadHtml("<html>" + node.InnerText + "</html>");
                            
                            n.Icon = HtmlManager.ExtractImage(docHtml);
                            n.Content = HtmlManager.ExtractText(docHtml);
                            n.Images = HtmlManager.ExtractImages(docHtml);
                        }                        
                    }

                    news.Add(n);
                }

                return news;
            }
        }
    }
    #endregion
}