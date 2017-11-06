using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SpExport.Util.Extention
{
    public static class ExtensionMethods
    {

        private static Action EmptyDelegate = delegate() { };


        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
    public static class BaseTemplate
    {
        public static bool  IsTemplateSupport(this int value)
        {
            switch (value)
            {
                case 100: //"GenericList";
                    return true;
                case 171: //"Task";
                    return true;
            }
            return false;
        }

        public static string GetTemplateBaseName(this int value)
        {
            var title = string.Empty;
            switch (value)
            {
                case 1:
                    title = "InvalidType";
                    break;
                case -1:
                    title = "NoListTemplate";
                    break;
                case 100:
                    title = "GenericList";
                    break;
                case 101:
                    title = "DocumentLibrary";
                    break;
                case 102:
                    title = "Survey";
                    break;
                case 103:
                    title = "Links";
                    break;
                case 104:
                    title = "Announcements";
                    break;
                case 105:
                    title = "Contacts";
                    break;
                case 106:
                    title = "Events";
                    break;
                case 107:
                    title = "Tasks";
                    break;
                case 108:
                    title = "DiscussionBoard";
                    break;
                case 109:
                    title = "PictureLibrary";
                    break;
                case 110:
                    title = "DataSources";
                    break;
                case 111:
                    title = "WebTemplateCatalog";
                    break;
                case 112:
                    title = "UserInformation";
                    break;
                case 113:
                    title = "WebPartCatalog";
                    break;
                case 114:
                    title = "ListTemplateCatalog";
                    break;

                case 115:
                    title = "XMLForm";
                    break;
                case 116:
                    title = "MasterPageCatalog";
                    break;
                case 117:
                    title = "NoCodeWorkflows";
                    break;
                case 118:
                    title = "WorkflowProcess";
                    break;
                case 119:
                    title = "WebPageLibrary";
                    break;
                case 120:
                    title = "CustomGrid";
                    break;
                case 121:
                    title = "SolutionCatalog";
                    break;
                case 122:
                    title = "NoCodePublic";
                    break;
                case 123:
                    title = "ThemeCatalog";
                    break;
                case 124:
                    title = "DesignCatalog";
                    break;
                case 125:
                    title = "AppDataCatalog";
                    break;
                case 130:
                    title = "DataConnectionLibrary";
                    break;
                case 140:
                    title = "WorkflowHistory";
                    break;
                case 150:
                    title = "GanttTasks";
                    break;
                case 151:
                    title = "HelpLibrary";
                    break;
                case 160:
                    title = "AccessRequest";
                    break;
                case 171:
                    title = "TasksWithTimelineAndHierarchy";
                    break;
                case 175:
                    title = "MaintenanceLogs";
                    break;
                case 200:
                    title = "Meetings";
                    break;
                case 201:
                    title = "Agenda";
                    break;
                case 202:
                    title = "MeetingUser";
                    break;
                case 204:
                    title = "Decision";
                    break;
                case 207:
                    title = "MeetingObjective";
                    break;
                case 210:
                    title = "TextBox";
                    break;
                case 211:
                    title = "ThingsToBring";
                    break;
                case 212:
                    title = "HomePageLibrary";
                    break;
                case 301:
                    title = "Posts";
                    break;
                case 302:
                    title = "Comments";
                    break;
                case 303:
                    title = "Categories";
                    break;
                case 402:
                    title = "Facility";
                    break;
                case 403:
                    title = "Whereabouts";
                    break;
                case 404:
                    title = "CallTrack";
                    break;
                case 405:
                    title = "Circulation";
                    break;
                case 420:
                    title = "Timecard";
                    break;
                case 421:
                    title = "Holidays";
                    break;
                case 499:
                    title = "IMEDic";
                    break;
                case 600:
                    title = "ExternalList";
                    break;
                case 700:
                    title = "MySiteDocumentLibrary";
                    break;
                case 1100:
                    title = "IssueTracking";
                    break;
                case 1200:
                    title = "AdminTasks";
                    break;
                case 1220:
                    title = "HealthRules";
                    break;
                case 1221:
                    title = "HealthReports";
                    break;
                case 1230:
                    title = "DeveloperSiteDraftApps";
                    break;

            }
            return title;
        }
    }
}
