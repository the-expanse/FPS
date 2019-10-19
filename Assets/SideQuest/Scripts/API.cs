using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;

public class API {
    public WebSocket ws;
    //SDK context;
    public int socketId;
    Dictionary<string, Action<JObject>> callbacks = new Dictionary<string, Action<JObject>>();
    System.Threading.Timer setUserValuesTimer = null;
    System.Threading.Timer oneShotTimer = null;
    System.Threading.Timer noopTimer = null;

    public API(SDK context) {
        //this.context = context;
    }

    public void UsersJoined(JObject response) {
        var users_ids = new JArray();
        var users = (JArray)response["data"];
        for (int i = 0; i < users.Count; i++) {
            users_ids.Add(new JValue((int)users[i]["data"]["user"]["users_id"]));
        }
    }

    public void Start(Action<JObject> start_callback) {
        ws = new WebSocket("wss://api.theexpanse.app");
        ws.Log.Level = LogLevel.Trace;
        ws.OnMessage += (sender, e) => {
            JsonReader reader = new JsonTextReader(new StringReader(e.Data));
            reader.DateParseHandling = DateParseHandling.None;
            JObject response = JObject.Load(reader);
            string path = (string)response["path"];
            if(path != "socketId") {
                SDK.RunOnMainThread.Enqueue(() => {
                    if (response["data"] != null && response["data"] is JObject) {
                        string token = (string)response["data"]["token"];
                        if (token != null) {
                            if (path == "generate-guest-login") {
                                PlayerPrefs.SetString("expanse_guest_token", token);
                            }
                            PlayerPrefs.SetString("expanse_token", token);
                            PlayerPrefs.Save();
                        }
                    }
                    switch (path) {
                        case "user-joined":
                            var users = new JArray();
                            users.Add(response["data"]);
                            var resp = new JObject();
                            resp["data"] = users;
                            UsersJoined(resp);
                            break;
                        case "users-joined":
                            UsersJoined(response);
                            break;
                        case "user-left":
                            //context.avatars.RemoveAvatar(response);
                            break;
                        case "kick-me":
                            //context.expanseSession.TeleportTo(1, () => { });
                            break;
                        case "user-message":
                            //if (((JObject)response["data"]["message"]).ContainsKey("is_blocked")) {
                            //    var key = context.avatars.GetSocketId((int)response["data"]["user"]["users_id"]);
                            //    context.avatars.Remove(key);
                            //    context.avatars.avatars.Remove(key);
                            //} else if (((JObject)response["data"]["message"]).ContainsKey("is_friend_request")) {
                            //    context.notifications.Notify(
                            //        "New Frient Request",
                            //        "You have received a freind request from " + (string)response["data"]["user"]["name"],
                            //        Notification.MessageType.NOTIFICATION,
                            //        () => {
                            //            context.notifications.DismissMessage();
                            //        });
                            //} else if (((JObject)response["data"]["message"]).ContainsKey("spaces_id")) {
                            //    int spaces_id = (int)response["data"]["message"]["spaces_id"];
                            //    for (int i = 0; i < context.notifications.Notifications.Count; i++) {
                            //        if (context.notifications.Notifications[i].type == Notification.MessageType.INVITE &&
                            //            context.notifications.Notifications[i].id == spaces_id) {
                            //            return;
                            //        }
                            //    }
                            //    var notification = context.notifications.Notify(
                            //        "Message From: " + (string)response["data"]["user"]["name"],
                            //        (string)response["data"]["message"]["text"],
                            //        Notification.MessageType.INVITE,
                            //        () => {
                            //            context.notifications.DismissMessage();
                            //            context.expanseSession.TeleportTo(spaces_id, () => { });
                            //        });
                            //    notification.id = spaces_id;

                            //} else  {
                            //    context.notifications.Notify(
                            //        "Message From: " + (string)response["data"]["user"]["name"],
                            //        (string)response["data"]["message"]["text"],
                            //        Notification.MessageType.INVITE,
                            //        () => {
                            //            context.notifications.DismissMessage();
                            //        });
                            //}
                            break;
                        case "one-shot":
                            switch ((string)response["data"]["type"]) {
                                case "emoji":
                                    //context.avatars.Emoji((int)response["data"]["socketId"], (string)response["data"]["data"]);
                                    break;
                                case "syncObject":
                                    //await this.editor.sceneGraph.syncReceive(data.data);
                                    break;
                                case "avatar-update":
                                    //context.avatars.RemoteAvatarUpdate((int)response["data"]["socketId"], (JObject)response["data"]["data"]);
                                    break;
                            }
                            break;
                        case "space-data":
                            //context.avatars.SetSpaceData(response);
                            break;
                        default:
                            //if (callbacks.ContainsKey(path)) {
                            //    var callback = callbacks[path];
                            //    callback(response);
                            //    callbacks.Remove(path);
                            //}else if (callbacks.ContainsKey(path.Replace("-err",""))) {
                            //    context.notifications.Notify(
                            //        "Request Error",
                            //        (string)response["data"],
                            //        Notification.MessageType.NOTIFICATION,
                            //        () =>context.notifications.DismissMessage());
                            //    callbacks.Remove(path.Replace("-err", ""));
                            //}
                            break;
                    };
                });
            } else {
                socketId = (int)response["data"];
            }
        };

        ws.OnOpen += (sender, e) => {
            Debug.Log("Connected to Expanse API");
            GetCurrentSession(session => {
                start_callback(session);
            });
        };

        ws.OnError += (sender, e) => {
            Debug.Log("Socket Error:"+ e.Message);
        };

        ws.OnClose += (sender, e) => {
            if (!isClosing) {
                Debug.Log("socket Close - reconnecting... ("+ e.Reason + ")");
                SDK.RunOnMainThread.Enqueue(() => {
                    ws.Connect();
                });
            }
        };

        ws.Connect();
    }
    public bool isClosing = false;
    JObject BuildEmit(string path, Action<JObject> callback = null){
        if (callback != null) {
            callbacks[path] = callback;
        }
        JObject body = new JObject();
        body.Add("path", new JValue(path));
        var token = PlayerPrefs.GetString("expanse_token");
        if(token != null){
            body.Add("token", new JValue(token));
        }
        Noop();
        return body;
    }

    public void emit(string path, int data, Action<JObject> callback = null){
        JObject body = BuildEmit(path, callback);
        body.Add("data", new JValue(data));
        if(ws.ReadyState == WebSocketSharp.WebSocketState.Open) {
            ws.Send(body.ToString());
        }
    }

    public void emit(string path, float data, Action<JObject> callback = null) {
        JObject body = BuildEmit(path, callback);
        body.Add("data", new JValue(data));
        if (ws.ReadyState == WebSocketSharp.WebSocketState.Open) {
            ws.Send(body.ToString());
        }
    }

    public void emit(string path, string data, Action<JObject> callback = null) {
        JObject body = BuildEmit(path, callback);
        body.Add("data", new JValue(data));
        if (ws.ReadyState == WebSocketSharp.WebSocketState.Open) {
            ws.Send(body.ToString());
        }
    }

    public void emit(string path, JObject data, Action<JObject> callback = null) {
        JObject body = BuildEmit(path, callback);
        body.Add("data", data);
        if (ws.ReadyState == WebSocketSharp.WebSocketState.Open) {
            ws.Send(body.ToString());
        }
    }

    public void ClearTempBlockList(Action<JObject> callback) {
        var options = new JObject();
        emit("clear-temp-blocks", options, callback);
    }

    public void GetCurrentSession(Action<JObject> callback) {
        var current_token = PlayerPrefs.GetString("expanse_token");
        if (current_token == "" || current_token == null) {
            var guestToken = PlayerPrefs.GetString("expanse_guest_token");
            var options = new JObject();
            if (guestToken != null && current_token != "") {
                options.Add("guestToken", new JValue(guestToken));
            }
            emit("generate-guest-login", options, callback);
        }else{
            Refresh(callback);
        }
    }

    public void Refresh(Action<JObject> callback) {
        var options = new JObject();
        emit("refresh", options, callback);
    }

    public void SendAvatarUpdate(Action<JObject> callback) {
        //var AvatarUpdate = new JObject();
        //AvatarUpdate["type"] = "avatar-update";
        //AvatarUpdate["data"] = new JObject();
        //AvatarUpdate["data"]["avatarMesh"] = (string)context.expanseSession.session["data"]["avatar_base_mesh"];
        //AvatarUpdate["data"]["avatarTexture"] = (string)context.expanseSession.session["data"]["image"];
        //AvatarUpdate["data"]["name"] = (string)context.expanseSession.session["data"]["name"];
        //AvatarUpdate["data"]["is_muted"] = (string)context.expanseSession.currentSettings["muteState"];
        //AvatarUpdate["data"]["handSize"] = (float)context.expanseSession.currentSettings["handSize"];
        //AvatarUpdate["data"]["handColor"] = (string)context.expanseSession.currentSettings["handColor"];
        //AvatarUpdate["data"]["laser"] = (string)context.expanseSession.currentSettings["laser"];
        //AvatarUpdate["data"]["laserColor"] = (string)context.expanseSession.currentSettings["laserColor"];
        //OneShot(AvatarUpdate);
    }

    public void Login(string login, string password, Action<JObject> callback){
        var options = new JObject();
        options.Add("login", new JValue(login));
        options.Add("password", new JValue(password));
        emit("login", options, callback);
    }

    public void SignUp(string name, string email, string password, string dob, Action<JObject> callback) {
        var options = new JObject();
        options.Add("name", new JValue(name));
        options.Add("email", new JValue(email));
        options.Add("password", new JValue(password));
        options.Add("dob", new JValue(dob));
        emit("sign-up", options, callback);
    }

    public void GetData(string socketId, Action<JObject> callback) {
        var options = new JObject();
        options.Add("socketId", new JValue(socketId));
        emit("get-data", options, callback);
    }

    public void GetEvent(int events_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("events_id", new JValue(events_id));
        emit("get-event", options, callback);
    }

    public void GetEvents(int page, string search, string filter, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        options.Add("filter", new JValue(filter));
        emit("events-list", options, callback);
    }

    public void GetMyEvents(int page, string search, string filter, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        options.Add("filter", new JValue(filter));
        emit("my-events", options, callback);
    }

    public void GetSpace(int spaces_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("spaces_id", new JValue(spaces_id));
        emit("space", options, callback);
    }

    public void GetSpaces(int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("spaces-list", options, callback);
    }

    public void GetMySpaces(int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("my-spaces", options, callback);
    }

    public void CreateSpace(JObject space, Action<JObject> callback) {
        emit("create-space", space, callback);
    }

    public void UpdateSpace(JObject space, Action<JObject> callback) {
        emit("update-space", space, callback);
    }

    public void DeleteSpace(int spaces_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("spaces_id", new JValue(spaces_id));
        emit("delete-space", options, callback);
    }

    public void GetScenes(int page, string search, int spaces_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("spaces_id", new JValue(spaces_id));
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("my-scenes", options, callback);
    }

    public void GetScene(int scenes_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("scenes_id", new JValue(scenes_id));
        emit("scene", options, callback);
    }

    public void CreateScene(string name, int files_id, string version, Action<JObject> callback) {
        var options = new JObject();
        options.Add("name", new JValue(name));
        options.Add("files_id", new JValue(files_id));
        options.Add("version", new JValue(version));
        emit("create-scene", options, callback);
    }

    public void UpdateScene(string name, int scenes_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("name", new JValue(name));
        options.Add("scenes_id", new JValue(scenes_id));
        emit("update-scene", options, callback);
    }

    public void GetNotifications(Action<JObject> callback) {
        var options = new JObject();
        emit("get-notifications", options, callback);
    }

    public void DeleteScene(int scenes_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("scenes_id", new JValue(scenes_id));
        Debug.Log(scenes_id);
        emit("delete-scene", options, callback);
    }

    public void GetMyPrefabs(int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("my-prefabs", options, callback);
    }

    public void CreatePrefab(string name, string description, string image, int files_id, bool is_public, bool obfuscate, Action<JObject> callback) {
        var options = new JObject();
        options.Add("name", new JValue(name));
        options.Add("description", new JValue(description));
        options.Add("image", new JValue(image));
        options.Add("files_id", new JValue(files_id));
        options.Add("is_public", new JValue(is_public));
        options.Add("obfuscate", new JValue(obfuscate));
        emit("create-prefab", options, callback);
    }

    public void CreatePrefab(int prefabs_id, string name, string description, string image, bool is_public, bool obfuscate, Action<JObject> callback) {
        var options = new JObject();
        options.Add("name", new JValue(name));
        options.Add("description", new JValue(description));
        options.Add("image", new JValue(image));
        options.Add("prefabs_id", new JValue(prefabs_id));
        options.Add("is_public", new JValue(is_public));
        options.Add("obfuscate", new JValue(obfuscate));
        emit("update-prefab", options, callback);
    }

    public void DeletePrefab(int prefabs_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("prefabs_id", new JValue(prefabs_id));
        emit("delete-prefab", options, callback);
    }

    public void GetOldBehaviours(JObject behaviours, bool old_id_check, Action<JObject> callback) {
        var options = new JObject();
        options.Add("behaviours", behaviours);
        options.Add("old_id_check", old_id_check);
        emit("get-scene-behaviours", options, callback);
    }

    public void GetSceneBehaviours(JObject behaviours, Action<JObject> callback) {
        var options = new JObject();
        options.Add("behaviours", behaviours);
        emit("get-scene-behaviours", options, callback);
    }

    public void SaveSceneBehaviours(JObject behaviours, Action<JObject> callback) {
        var options = new JObject();
        options.Add("behaviours", behaviours);
        emit("save-scene-behaviours", options, callback);
    }

    public void CreateBehaviour(string name, string description, string image, string definition, 
        bool is_public, bool obfuscate, bool sync, string trigger, Action<JObject> callback) {
        var options = new JObject();
        options.Add("name", new JValue(name));
        options.Add("description", new JValue(description));
        options.Add("image", new JValue(image));
        options.Add("definition", new JValue(definition));
        options.Add("is_public", new JValue(is_public));
        options.Add("obfuscate", new JValue(obfuscate));
        options.Add("sync", new JValue(sync));
        options.Add("trigger", new JValue(trigger));
        emit("create-behaviour", options, callback);
    }

    public void UpdateBehaviour(int behaviours_id, string name, string description, string image, string definition,
        bool is_public, bool obfuscate, bool sync, string trigger, Action<JObject> callback) {
        var options = new JObject();
        options.Add("behaviours_id", new JValue(behaviours_id));
        options.Add("name", new JValue(name));
        options.Add("description", new JValue(description));
        options.Add("image", new JValue(image));
        options.Add("definition", new JValue(definition));
        options.Add("is_public", new JValue(is_public));
        options.Add("obfuscate", new JValue(obfuscate));
        options.Add("sync", new JValue(sync));
        options.Add("trigger", new JValue(trigger));
        emit("create-behaviour", options, callback);
    }

    public void SaveSceneBehaviours(int page, JObject excluded, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("excluded", excluded);
        emit("behaviours-not-including", options, callback);
    }

    public void GetMyBehaviours(int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("my-behaviours", options, callback);
    }

    public void DeleteBehaviour(int behaviours_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("behaviours_id", new JValue(behaviours_id));
        emit("delete-behaviour", options, callback);
    }

    public void CreateEvent(JObject _event, Action<JObject> callback) {
        var options = new JObject();
        options.Add("event", _event);
        emit("create-event", options, callback);
    }

    public void UpdateEvent(JObject _event, Action<JObject> callback) {
        var options = new JObject();
        options.Add("event", _event);
        emit("update-event", options, callback);
    }

    public void DeleteEvent(int events_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("events_id", new JValue(events_id));
        emit("delete-event", options, callback);
    }

    public void GetPeopleInSpace(bool currentSpace, int spaceId, int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("currentSpace", new JValue(currentSpace));
        options.Add("spaceId", new JValue(spaceId));
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("people-in-space", options, callback);
    }

    public void GetFriends( int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("friends", options, callback);
    }

    public void AddFriend(int userId, Action<JObject> callback) {
        var options = new JObject();
        options.Add("userId", new JValue(userId));
        emit("add-friend", options, callback);
    }

    public void RemoveFriend(int userId, Action<JObject> callback) {
        var options = new JObject();
        options.Add("userId", new JValue(userId));
        emit("reject-request", options, callback);
    }

    public void GetFriendRequests(int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("requests", options, callback);
    }

    public void AcceptFriendRequest(int userId, Action<JObject> callback) {
        var options = new JObject();
        options.Add("userId", new JValue(userId));
        emit("accept-request", options, callback);
    }

    public void RejectFriendRequest(int userId, Action<JObject> callback) {
        var options = new JObject();
        options.Add("userId", new JValue(userId));
        emit("reject-request", options, callback);
    }

    public void GetBlocked(int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("blocked", options, callback);
    }

    public void BlockUser(int userId, int type, Action<JObject> callback) {
        var options = new JObject();
        options.Add("userId", new JValue(userId));
        options.Add("type", new JValue(type));
        emit("block-user", options, callback);
    }

    public void UnBlockUser(int userId, Action<JObject> callback) {
        var options = new JObject();
        options.Add("userId", new JValue(userId));
        emit("unblock-user", options, callback);
    }

    public void ReportUser(int userId, string type, string details, Action<JObject> callback) {
        var options = new JObject();
        options.Add("userId", new JValue(userId));
        options.Add("type", new JValue(type));
        options.Add("details", new JValue(details));
        emit("report-user", options, callback);
    }

    public void GetMessagesPeople(int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("messages-people", options, callback);
    }

    public void GetMessagesThread(int userId, int page, string search, Action<JObject> callback) {
        var options = new JObject();
        options.Add("userId", new JValue(userId));
        options.Add("page", new JValue(page));
        options.Add("search", new JValue(search));
        emit("messages-thread", options, callback);
    }

    public void KickUser(int userId) {
        var options = new JObject();
        options.Add("users_id", new JValue(userId));
        emit("kick-user", options);
    }

    public void FriendUser(int userId) {
        var message = new JObject();
        message.Add("is_friend_request", new JValue(true));
        var options = new JObject();
        options.Add("users_id", new JValue(userId));
        options.Add("message", message);
        emit("user-message", options);
    }

    public void SendBlockUser(int userId) {
        var message = new JObject();
        message.Add("is_blocked", new JValue(true));
        var options = new JObject();
        options.Add("users_id", new JValue(userId));
        options.Add("message", message);
        emit("user-message", options);
    }

    public void InviteUser(int userId, JObject space) {
        var message = new JObject();
        message.Add("text", "Come visit me at " + (string)space["name"] + "!");
        message.Add("spaces_id", new JValue((int)space["spaces_id"]));

        var options = new JObject();
        options.Add("users_id", new JValue(userId));
        options.Add("message", message);
        emit("user-message", options);
    }

    public void IsBlocked(JArray users_ids, Action<JObject> callback) {
        var options = new JObject();
        options.Add("users_ids", users_ids);
        emit("is-blocked", options, callback);
    }

    public void SendMessage(int userId, string _message, Action<JObject> callback) {
        var message = new JObject();
        message.Add("text", _message);

        var options = new JObject();
        options.Add("users_id", new JValue(userId));
        options.Add("message", message);
        emit("user-message", options);
        var msg = new JObject();
        msg.Add("userId", userId);
        msg.Add("message", _message);
        emit("send-message", msg, callback);
    }

    public void ForgotPassword(string email, Action<JObject> callback) {
        var options = new JObject();
        options.Add("email", new JValue(email));
        emit("forgot-password", options, callback);
    }

    public void ResetPassword(string password, string resetToken, Action<JObject> callback) {
        var options = new JObject();
        options.Add("password", new JValue(password));
        options.Add("resetToken", new JValue(resetToken));
        emit("reset-password", options, callback);
    }

    public void GetUserWithSpace(Action<JObject> callback) {
        var options = new JObject();
        emit("get-user-with-space", options, callback);
    }

    public void GetUserCurrentSpace(int users_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("users_id", new JValue(users_id));
        emit("get-user-current-space", options, callback);
    }

    public void SaveUserDetails(string name, string email, Action<JObject> callback) {
        var options = new JObject();
        options.Add("name", new JValue(name));
        options.Add("email", new JValue(email));
        emit("save-user-details", options, callback);
    }

    public void SaveUserDefaultSpace(int spaceId, Action<JObject> callback) {
        var options = new JObject();
        options.Add("spaceId", new JValue(spaceId));
        emit("save-user-default-space", options, callback);
    }

    public void SaveUserPassword(string password, Action<JObject> callback) {
        var options = new JObject();
        options.Add("password", new JValue(password));
        emit("save-user-password", options, callback);
    }

    public void GetAvatarImages(Action<JObject> callback) {
        var options = new JObject();
        emit("get-avatar-images", options, callback);
    }

    public void SetUserAvatarImage(int avatar_images_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("avatar_images_id", new JValue(avatar_images_id));
        emit("set-user-avatar-image", options, callback);
    }

    public void SaveAvatarImage(string image, string preview, Action<JObject> callback) {
        var options = new JObject();
        options.Add("image", new JValue(image));
        options.Add("preview", new JValue(preview));
        emit("save-avatar-images", options, callback);
    }

    public void DeleteAvatarImage(int avatar_images_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("avatar_images_id", new JValue(avatar_images_id));
        emit("delete-avatar-images", options, callback);
    }

    public void SetDefaultAvatar(string geometry, string texture, Action<JObject> callback) {
        var options = new JObject();
        options.Add("geometry", new JValue(geometry));
        options.Add("texture", new JValue(texture));
        emit("set-default-avatar", options, callback);
    }

    public void GetUserTimezone(int users_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("users_id", new JValue(users_id));
        emit("get-user-timezone", options, callback);
    }


    JValue GetRelated(JObject resp, string value) {
        return new JValue(resp["data"][value].Type != JTokenType.Null && (bool)resp["data"][value]);
    }
    
    public void GetRelated(JObject user, Action callback) {
        GetRelated((int)user["users_id"], resp => {
            user["is_related"] = GetRelated(resp, "is_related");
            user["is_blocked"] = GetRelated(resp, "is_blocked");
            user["is_accepted"] = GetRelated(resp, "is_accepted");
            user["initiated"] = GetRelated(resp, "initiated");
            callback();
        });
    }

    public void GetRelated(int users_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("users_id", new JValue(users_id));
        emit("get-related", options, callback);
    }

    public void MessageRead(int messages_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("messagesId", new JValue(messages_id));
        emit("message-read", options, callback);
    }

    public void RemoveSpaceValue(string key, bool isTransient, Action<JObject> callback) {
        var options = new JObject();
        options.Add("key", new JValue(key));
        options.Add("isTransient", new JValue(isTransient));
        emit("remove-space-value", options, callback);
    }

    public void GetSpaceValue(string key, bool isTransient, Action<JObject> callback) {
        var options = new JObject();
        options.Add("key", new JValue(key));
        options.Add("isTransient", new JValue(isTransient));
        emit("get-space-value", options, callback);
    }

    public void SetSpaceValue(string key, JObject value, bool isTransient, Action<JObject> callback) {
        var options = new JObject();
        options.Add("key", new JValue(key));
        options.Add("value", new JValue(value.ToString(Formatting.None)));
        options.Add("isTransient", new JValue(isTransient));
        emit("set-space-value", options, callback);
    }

    public void SetSpace(int spaces_id, Action<JObject> callback) {
        var options = new JObject();
        options.Add("spaces_id", new JValue(spaces_id));
        emit("set-space", options, callback);
    }

    public void OneShot(JObject data) {
        if (oneShotTimer != null) {
            oneShotTimer.Dispose();
        }
        oneShotTimer = new System.Threading.Timer((obj) => {
            SDK.RunOnMainThread.Enqueue(() => {
                emit("one-shot", data);
            });
            oneShotTimer.Dispose();
        }, null, 750, System.Threading.Timeout.Infinite);
    }

    public void Noop() {
        var options = new JObject();
        if (noopTimer != null) {
            noopTimer.Dispose();
        }
        noopTimer = new System.Threading.Timer((obj) => {
            SDK.RunOnMainThread.Enqueue(() => {
                emit("no-op", options);
                noopTimer.Dispose();
                Noop();
            });
        }, null, 25000, System.Threading.Timeout.Infinite);
    }

    public void SyncObject(JObject data, Action<JObject> callback) {
        emit("sync-object", data, callback);
    }

    public void GetUserValues(Action<JObject> callback) {
        emit("get-user-values", new JObject(), callback);
    }

    private void SetUserValue(JObject value, Action<JObject> callback) {
        var array = new JArray();
        array.Add(value);
        SetUserValues(array, callback);
    }

    public void SetUserValue(string key, bool value, Action<JObject> callback) {
        var values = new JObject();
        values["key"] = key;
        values["value"] = new JValue(value);
        SetUserValue(values, callback);
    }

    public void SetUserValue(string key, float value, Action<JObject> callback) {
        var values = new JObject();
        values["key"] = key;
        values["value"] = new JValue(value);
        SetUserValue(values, callback);
    }

    public void SetUserValue(string key, int value, Action<JObject> callback) {
        var values = new JObject();
        values["key"] = key;
        values["value"] = new JValue(value);
        SetUserValue(values, callback);
    }

    public void SetUserValue(string key, string value, Action<JObject> callback) {
        var values = new JObject();
        values["key"] = key;
        values["value"] = new JValue(value);
        SetUserValue(values, callback);
    }

    public void SetUserValues(JArray keyValues, Action<JObject> callback) {
        var options = new JObject();
        options.Add("keyValues", keyValues);
        if(setUserValuesTimer != null) {
            setUserValuesTimer.Dispose();
        }
        setUserValuesTimer = new System.Threading.Timer((obj) => {
            SDK.RunOnMainThread.Enqueue(() => {
                emit("set-user-values", options, callback);
            });
            setUserValuesTimer.Dispose();
        },null, 1500, System.Threading.Timeout.Infinite);
    }

    public void HeartBeat(Action<JObject> callback) {
        emit("heart-beat", new JObject(), callback);
    }
}
