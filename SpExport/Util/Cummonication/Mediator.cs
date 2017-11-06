using System;

namespace SpExport.Util.Cummonication
{

    /// <summary>
    /// Available cross ViewModel messages
    /// </summary>
    public enum ViewModelMessages
    {
        Connect = 1,
        IndexPage=2,
 
    
    };


     public sealed class Mediator
     {
          #region Data
          static readonly Mediator instance = new Mediator();
        private volatile object _locker = new object();
  
          MultiDictionary<ViewModelMessages, Action<Object>> internalList
             = new MultiDictionary<ViewModelMessages, Action<Object>>();
           #endregion
  
          #region Ctor
          //CTORs
         static Mediator()
          {

 
         }

         public Mediator()
         {
 
         }
          #endregion
 
         #region Public Properties
 
         /// <summary>
          /// The singleton instance
         /// </summary>
         public static Mediator Instance
         {
             get
             {
                 return instance;
               }
          }
  
           #endregion
   
          #region Public Methods
         /// <summary>
           /// Registers a Colleague to a specific message
           /// </summary>
         /// <param name="callback">The callback to use 
            /// when the message it seen</param>
           /// <param name="message">The message to 
           /// register to</param>
            public void Register(Action<Object> callback, 
               ViewModelMessages message)
           {
               internalList.AddValue(message, callback);
          }
   
  
          /// <summary>
            /// Notify all colleagues that are registed to the 
            /// specific message
           /// </summary>
            /// <param name="message">The message for the notify by</param>
            /// <param name="args">The arguments for the message</param>
            public void NotifyColleagues(ViewModelMessages message, 
                object args)
            {
                if (internalList.ContainsKey(message))
               {
                   //forward the message to all listeners
                   foreach (Action<object> callback in 
                       internalList[message])
                           callback(args);
                }
            }
            #endregion
   
        }
    }

