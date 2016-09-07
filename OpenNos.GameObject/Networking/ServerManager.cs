﻿/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using AutoMapper;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ServerManager : BroadcastableBase
    {
        #region Members

        public Boolean ShutdownStop = false;

        private static ServerManager _instance;
        private static List<Item> _items = new List<Item>();
        private static IMapper _mapper;
        private static ConcurrentDictionary<Guid, Map> _maps = new ConcurrentDictionary<Guid, Map>();
        private static List<NpcMonster> _npcs = new List<NpcMonster>();
        private static List<Skill> _skills = new List<Skill>();
        private long lastGroupId;

        #endregion

        #region Instantiation

        static ServerManager()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ItemDTO, NoFunctionItem>();
                cfg.CreateMap<ItemDTO, WearableItem>();
                cfg.CreateMap<ItemDTO, BoxItem>();
                cfg.CreateMap<ItemDTO, MagicalItem>();
                cfg.CreateMap<ItemDTO, FoodItem>();
                cfg.CreateMap<ItemDTO, PotionItem>();
                cfg.CreateMap<ItemDTO, ProduceItem>();
                cfg.CreateMap<ItemDTO, SnackItem>();
                cfg.CreateMap<ItemDTO, SpecialItem>();
                cfg.CreateMap<ItemDTO, TeacherItem>();
                cfg.CreateMap<ItemDTO, UpgradeItem>();
                cfg.CreateMap<SkillDTO, Skill>();
                cfg.CreateMap<ComboDTO, Combo>();
                cfg.CreateMap<NpcMonsterDTO, NpcMonster>();
            });

            _mapper = config.CreateMapper();
        }

        private ServerManager()
        {
            Groups = new List<Group>();

            Task autosave = new Task(SaveAllProcess);
            autosave.Start();

            Task GroupTask = new Task(() => GroupProcess());
            GroupTask.Start();

            Task TaskController = new Task(() => TaskLauncherProcess());
            TaskController.Start();
            lastGroupId = 1;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ItemDTO, NoFunctionItem>();
                cfg.CreateMap<ItemDTO, WearableItem>();
                cfg.CreateMap<ItemDTO, BoxItem>();
                cfg.CreateMap<ItemDTO, MagicalItem>();
                cfg.CreateMap<ItemDTO, FoodItem>();
                cfg.CreateMap<ItemDTO, PotionItem>();
                cfg.CreateMap<ItemDTO, ProduceItem>();
                cfg.CreateMap<ItemDTO, SnackItem>();
                cfg.CreateMap<ItemDTO, SpecialItem>();
                cfg.CreateMap<ItemDTO, TeacherItem>();
                cfg.CreateMap<ItemDTO, UpgradeItem>();
                cfg.CreateMap<SkillDTO, Skill>();
                cfg.CreateMap<ComboDTO, Combo>();
                cfg.CreateMap<NpcMonsterDTO, NpcMonster>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Properties

        public static int DropRate { get; set; }
        public static int FairyXpRate { get; set; }
        public static int GoldRate { get; set; }
        public static List<MapMonster> Monsters { get; set; }
        public static EventHandler NotifyChildren { get; set; }
        public static int XPRate { get; set; }
        public List<Group> Groups { get; set; }
        public static ServerManager Instance => _instance ?? (_instance = new ServerManager());
        public Task TaskShutdown { get; set; }

        #endregion

        #region Methods

        public static ConcurrentDictionary<Guid, Map> GetAllMap()
        {
            return _maps;
        }

        public static IEnumerable<Skill> GetAllSkill()
        {
            return _skills;
        }

        public static Item GetItem(short vnum)
        {
            return _items.FirstOrDefault(m => m.VNum.Equals(vnum));
        }

        public static Map GetMap(short id)
        {
            return _maps.FirstOrDefault(m => m.Value.MapId.Equals(id)).Value;
        }

        public static NpcMonster GetNpc(short npcVNum)
        {
            return _npcs.FirstOrDefault(m => m.NpcMonsterVNum.Equals(npcVNum));
        }

        public static Skill GetSkill(short skillVNum)
        {
            return _skills.FirstOrDefault(m => m.SkillVNum.Equals(skillVNum));
        }

        public static void Initialize()
        {
            XPRate = int.Parse(System.Configuration.ConfigurationManager.AppSettings["RateXp"]);

            DropRate = int.Parse(System.Configuration.ConfigurationManager.AppSettings["RateDrop"]);

            GoldRate = int.Parse(System.Configuration.ConfigurationManager.AppSettings["RateGold"]);

            FairyXpRate = int.Parse(System.Configuration.ConfigurationManager.AppSettings["RateFairyXp"]);

            foreach (ItemDTO itemDTO in DAOFactory.ItemDAO.LoadAll())
            {
                Item ItemGO = null;

                switch (itemDTO.ItemType)
                {
                    case (byte)Domain.ItemType.Ammo:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Armor:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Box:
                        ItemGO = _mapper.Map<BoxItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Event:
                        ItemGO = _mapper.Map<MagicalItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Fashion:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Food:
                        ItemGO = _mapper.Map<FoodItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Jewelery:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Magical:
                        ItemGO = _mapper.Map<MagicalItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Main:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Map:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Part:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Potion:
                        ItemGO = _mapper.Map<PotionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Production:
                        ItemGO = _mapper.Map<ProduceItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Quest1:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Quest2:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Sell:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Shell:
                        ItemGO = _mapper.Map<MagicalItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Snack:
                        ItemGO = _mapper.Map<SnackItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Special:
                        ItemGO = _mapper.Map<SpecialItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Specialist:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Teacher:
                        ItemGO = _mapper.Map<TeacherItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Upgrade:
                        ItemGO = _mapper.Map<UpgradeItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Weapon:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    default:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;
                }
                _items.Add(ItemGO);
            }

            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("ITEM_LOADED"), _items.Count()));
            foreach (SkillDTO skillDTO in DAOFactory.SkillDAO.LoadAll())
            {
                Skill skill = _mapper.Map<Skill>(skillDTO);
                foreach (ComboDTO com in DAOFactory.ComboDAO.LoadBySkillVnum(skill.SkillVNum))
                {
                    skill.Combos.Add(_mapper.Map<Combo>(com));
                }
                _skills.Add(skill);
            }
            foreach (NpcMonsterDTO npcmonsterDTO in DAOFactory.NpcMonsterDAO.LoadAll())
            {
                _npcs.Add(_mapper.Map<NpcMonster>(npcmonsterDTO));
            }
            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("NPCMONSTERS_LOADED"), _npcs.Count()));

            try
            {
                int i = 0;
                int npccount = 0;
                int recipescount = 0;
                int shopcount = 0;
                int monstercount = 0;
                Monsters = new List<MapMonster>();
                foreach (MapDTO map in DAOFactory.MapDAO.LoadAll())
                {
                    Guid guid = Guid.NewGuid();
                    Map newMap = new Map(Convert.ToInt16(map.MapId), guid, map.Data);
                    newMap.Music = map.Music;
                    //register for broadcast
                    _maps.TryAdd(guid, newMap);
                    i++;
                    npccount += newMap.Npcs.Count();

                    foreach (MapMonster n in newMap.Monsters)
                        Monsters.Add(n);
                    monstercount += newMap.Monsters.Count();
                    foreach (MapNpc n in newMap.Npcs.Where(n => n.Shop != null))
                        shopcount++;
                    foreach (MapNpc n in newMap.Npcs)
                        foreach (Recipe n2 in n.Recipes)
                            recipescount++;
                }
                if (i != 0)
                    Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MAP_LOADED"), i));
                else
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("NO_MAP"));

                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("SKILLS_LOADED"), _skills.Count()));
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MONSTERS_LOADED"), monstercount));
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("NPCS_LOADED"), npccount));
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("SHOPS_LOADED"), shopcount));
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("RECIPES_LOADED"), recipescount));
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
        }

        public static async void MemoryWatch(string type)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            while (true)
            {
                Console.Title = $"{type} v{fileVersionInfo.ProductVersion} - Memory: {GC.GetTotalMemory(true) / (1024 * 1024)}MB";
                await Task.Delay(1000);
            }
        }

        public static void OnBroadCast(BroacastPacket mapPacket)
        {
            NotifyChildren?.Invoke(mapPacket, new EventArgs());
        }

        //PacketHandler -> with Callback?
        public void AskRevive(long Target)
        {
            ClientSession Session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == Target);
            if (Session != null && Session.Character != null)
            {
                Session.Client.SendPacket("cancel 0 0");
                Session.Client.SendPacket("cancel 2 0");
                Session.Client.SendPacket(Session.Character.GenerateStat());
                Session.Client.SendPacket("vb 340 0 0");
                Session.Client.SendPacket("vb 339 0 0");
                Session.Client.SendPacket("vb 472 0 0");
                Session.Client.SendPacket("vb 471 0 0");
                if (Session.Character.Level > 20)
                {
                    Session.Character.Dignity -= (short)(Session.Character.Level < 50 ? Session.Character.Level : 50);
                    if (Session.Character.Dignity < -1000)
                        Session.Character.Dignity = -1000;

                    Session.Client.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LOSE_DIGNITY"), (short)(Session.Character.Level < 50 ? Session.Character.Level : 50)), 11));
                    Session.Client.SendPacket(Session.Character.GenerateFd());
                }
                Session.Client.SendPacket("eff_ob -1 -1 0 4269");
                Session.Client.SendPacket(Session.Character.GenerateDialog($"#revival^0 #revival^1 {(Session.Character.Level > 20 ? Language.Instance.GetMessageFromKey("ASK_REVIVE") : Language.Instance.GetMessageFromKey("ASK_REVIVE_FREE"))}"));

                Task.Factory.StartNew(async () =>
                {
                    for (int i = 1; i <= 30; i++)
                    {
                        await Task.Delay(1000);
                        if (Session.Character.Hp > 0)
                            return;
                    }
                    Instance.ReviveFirstPosition(Session.Character.CharacterId);
                });
            }
        }

        //PacketHandler
        public void BuyValidate(ClientSession clientSession, KeyValuePair<long, MapShop> shop, short slot, byte amount)
        {
            PersonalShopItem itemshop = clientSession.CurrentMap.UserShops[shop.Key].Items.FirstOrDefault(i => i.Slot.Equals(slot));
            if (itemshop == null)
                return;
            Guid id = itemshop.Id;
            itemshop.Amount -= amount;
            if (itemshop.Amount <= 0)
                clientSession.CurrentMap.UserShops[shop.Key].Items.Remove(itemshop);

            ClientSession shopOwnerSession = Sessions.FirstOrDefault(s => s.Character.CharacterId.Equals(shop.Value.OwnerId));
            if (shopOwnerSession == null) return;

            shopOwnerSession.Character.Gold += itemshop.Price * amount;
            shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateGold());
            shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateShopMemo(1,
                string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM"), shopOwnerSession.Character.Name, (itemshop.ItemInstance as ItemInstance).Item.Name, amount)));
            clientSession.CurrentMap.UserShops[shop.Key].Sell += itemshop.Price * amount;
            shopOwnerSession.Client.SendPacket($"sell_list {shop.Value.Sell} {slot}.{amount}.{itemshop.Amount}");

            Inventory inv = shopOwnerSession.Character.InventoryList.RemoveItemAmountFromInventory(amount, id);

            if (inv != null)
            {
                // Send reduced-amount to owners inventory
                shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade, 0));
            }
            else
            {
                // Send empty slot to owners inventory
                shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(-1, 0, itemshop.Type, itemshop.Slot, 0, 0, 0, 0));
                if (clientSession.CurrentMap.UserShops[shop.Key].Items.Count == 0)
                {
                    clientSession.Client.SendPacket("shop_end 0");

                    Broadcast(shopOwnerSession, shopOwnerSession.Character.GenerateShopEnd(), ReceiverType.All);
                    Broadcast(shopOwnerSession, shopOwnerSession.Character.GeneratePlayerFlag(0), ReceiverType.AllExceptMe);
                    shopOwnerSession.Character.Speed = shopOwnerSession.Character.LastSpeed != 0 ? shopOwnerSession.Character.LastSpeed : shopOwnerSession.Character.Speed;
                    shopOwnerSession.Character.IsSitting = false;
                    shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateCond());
                    Broadcast(shopOwnerSession, shopOwnerSession.Character.GenerateRest(), ReceiverType.All);
                }
            }
        }

        //Both partly
        public void ChangeMap(long id)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == id);
            if (session != null)
            {
                session.CurrentMap.UnregisterSession(session);
                session.CurrentMap = ServerManager.GetMap(session.Character.MapId);
                session.CurrentMap.RegisterSession(session);
                session.Client.SendPacket(session.Character.GenerateCInfo());
                session.Client.SendPacket(session.Character.GenerateCMode());
                session.CurrentMap?.Broadcast(session, session.Character.GenerateEq(), ReceiverType.All);
                session.Client.SendPacket(session.Character.GenerateEquipment());
                session.Client.SendPacket(session.Character.GenerateLev());
                session.Client.SendPacket(session.Character.GenerateStat());
                session.Client.SendPacket(session.Character.GenerateAt());
                session.Client.SendPacket(session.Character.GenerateCond());
                session.Client.SendPacket(session.Character.GenerateCMap());
                session.Client.SendPacket(session.Character.GenerateStatChar());
                session.Client.SendPacket($"gidx 1 {session.Character.CharacterId} -1 - 0"); // family
                session.Client.SendPacket("rsfp 0 -1");
                //cond 2 // partner only send when partner present
                session.Client.SendPacket("pinit 0"); // clean party list
                session.Client.SendPacket(session.Character.GeneratePairy());
                session.CurrentMap?.Broadcast(session, session.Character.GeneratePairy(), ReceiverType.AllExceptMe);
                session.Client.SendPacket("act6"); // act6 1 0 14 0 0 0 14 0 0 0

                foreach (String portalPacket in session.Character.GenerateGp())
                    session.Client.SendPacket(portalPacket);
                // wp 23 124 4 4 12 99
                foreach (String monsterPacket in session.Character.GenerateIn3())
                    session.Client.SendPacket(monsterPacket);
                foreach (String npcPacket in session.Character.GenerateIn2())
                    session.Client.SendPacket(npcPacket);
                foreach (String ShopPacket in session.Character.GenerateNPCShopOnMap())
                    session.Client.SendPacket(ShopPacket);
                foreach (String droppedPacket in session.Character.GenerateDroppedItem())
                    session.Client.SendPacket(droppedPacket);
                foreach (String ShopPacket in session.Character.GenerateShopOnMap())
                    session.Client.SendPacket(ShopPacket);
                foreach (String ShopPacketChar in session.Character.GeneratePlayerShopOnMap())
                    session.Client.SendPacket(ShopPacketChar);
                ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(session.Character.MapId) && s.Character.Name != session.Character.Name && !s.Character.InvisibleGm).ToList().ForEach(s => RequireBroadcastFromUser(session, s.Character.CharacterId, "GenerateIn"));
                if (session.Character.InvisibleGm == false)
                    session.CurrentMap?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                if (session.Character.Size != 10)
                    session.Client.SendPacket(session.Character.GenerateScal());
                if (session.CurrentMap.IsDancing == 2 && session.Character.IsDancing == 0)
                    session.CurrentMap?.Broadcast("dance 2");
                else if (session.CurrentMap.IsDancing == 0 && session.Character.IsDancing == 1)
                {
                    session.Character.IsDancing = 0;
                    session.CurrentMap?.Broadcast("dance");
                }
                foreach (Group g in Groups)
                {
                    foreach (ClientSession groupSession in g.Characters)
                    {
                        ClientSession chara = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == groupSession.Character.CharacterId && s.CurrentMap.MapId == groupSession.CurrentMap.MapId);
                        if (chara != null)
                        {
                            groupSession.Client.SendPacket(GeneratePinit(groupSession.Character.CharacterId));
                        }
                        if (groupSession.Character.CharacterId == groupSession.Character.CharacterId)
                        {
                            session.CurrentMap?.Broadcast(groupSession, GeneratePidx(groupSession.Character.CharacterId), ReceiverType.AllExceptMe);
                        }
                    }
                }
            }
        }

        //PacketHandler
        public void ExchangeValidate(ClientSession c1Session, long charId)
        {
            ClientSession c2Session = Sessions.FirstOrDefault(s => s.Character.CharacterId.Equals(charId));
            {
                if (c2Session == null) return;

                foreach (ItemInstance item in c2Session.Character.ExchangeInfo.ExchangeList)
                {
                    Inventory invtemp = c2Session.Character.InventoryList.Inventory.FirstOrDefault(s => s.ItemInstance.Id == item.Id);
                    short slot = invtemp.Slot;
                    byte type = invtemp.Type;

                    Inventory inv = c2Session.Character.InventoryList.RemoveItemAmountFromInventory((byte)item.Amount, invtemp.Id);
                    if (inv != null)
                    {
                        // Send reduced-amount to owners inventory
                        c2Session.Client.SendPacket(c2Session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade, 0));
                    }
                    else
                    {
                        // Send empty slot to owners inventory
                        c2Session.Client.SendPacket(c2Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0, 0));
                    }
                }

                foreach (ItemInstance item in c1Session.Character.ExchangeInfo.ExchangeList)
                {
                    ItemInstance item2 = item.DeepCopy();
                    item2.Id = Guid.NewGuid();
                    Inventory inv = c2Session.Character.InventoryList.AddToInventory(item2);
                    if (inv == null) continue;
                    if (inv.Slot == -1) continue;
                    c2Session.Client.SendPacket(c2Session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade, 0));
                }

                c2Session.Character.Gold = c2Session.Character.Gold - c2Session.Character.ExchangeInfo.Gold + c1Session.Character.ExchangeInfo.Gold;
                c2Session.Client.SendPacket(c2Session.Character.GenerateGold());
                c1Session.Character.ExchangeInfo = null;
                c2Session.Character.ExchangeInfo = null;
            }
        }

        public string GeneratePidx(long charId)
        {
            int? count = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId)).Characters?.Select(c => c.Character.CharacterId).Count();
            string str = "";
            if (count != null)
            {
                str = $"pidx {count}";
                int i = 0;
                foreach (long Id in ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId)).Characters?.Select(c => c.Character.CharacterId))
                {
                    i++;
                    str += $" {i}.{Id} ";
                }
            }
            if (str == $"pidx {count}")
                str = "";
            return str;
        }

        public string GeneratePinit(long charId)
        {
            Group grp = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId));

            string str = $"pinit {grp.Characters.Count()}";
            int i = 0;
            foreach (ClientSession groupSessionForId in grp.Characters)
            {
                i++;
                str += $" 1|{groupSessionForId.Character.CharacterId}|{i}|{groupSessionForId.Character.Level}|{groupSessionForId.Character.Name}|0|{groupSessionForId.Character.Gender}|{groupSessionForId.Character.Class}|{(groupSessionForId.Character.UseSp ? groupSessionForId.Character.Morph : 0)}|{groupSessionForId.Character.HeroLevel}";
            }
            if (str == $"pinit {grp.Characters.Count()}")
                str = "";
            return str;
        }

        public long GetNextGroupId()
        {
            lastGroupId++;
            return lastGroupId;
        }

        public T GetProperty<T>(string charName, string property)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(charName));
            if (session == null)
                return default(T);
            return (T)session?.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public T GetProperty<T>(long charId, string property)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(charId));
            if (session == null)
                return default(T);
            return (T)session?.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public T GetUserMethod<T>(long characterId, string methodName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(characterId));
            if (session == null) return default(T);
            MethodInfo method = session.Character.GetType().GetMethod(methodName);

            return (T)method.Invoke(session.Character, null);
        }

        public void GroupLeave(ClientSession session)
        {
            Group grp = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(session.Character.CharacterId));
            if (grp != null)
            {
                if (grp.Characters.Count() == 3)
                {
                    if (grp.Characters.ElementAt(0) == session)
                    {
                        Broadcast(session, session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("NEW_LEADER")), ReceiverType.OnlySomeone, "", grp.Characters.ElementAt(1).Character.CharacterId);
                    }
                    grp.LeaveGroup(session);
                    foreach (ClientSession groupSession in grp.Characters)
                    {
                        foreach (ClientSession sess in Sessions.Where(s => s != null && s.Character != null && s.Character.CharacterId == groupSession.Character.CharacterId))
                        {
                            sess.Client.SendPacket(GeneratePinit(groupSession.Character.CharacterId));
                            sess.Client.SendPacket(sess.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("LEAVE_GROUP"), session.Character.Name), 0));
                        }
                    }
                    session.Client.SendPacket($"pidx -1 1.{ session.Character.CharacterId}");
                    Broadcast(session, $"pidx -1 1.{session.Character.CharacterId}", ReceiverType.AllExceptMe);
                    session.Client.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_LEFT"), 0));
                }
                else
                {
                    foreach (ClientSession targetSession in grp.Characters)
                    {
                        foreach (ClientSession sess in Sessions.Where(s => s != null && s.Character != null && s.Character.CharacterId == targetSession.Character.CharacterId))
                        {
                            sess.Client.SendPacket("pinit 0");
                            sess.Client.SendPacket(sess.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_CLOSED"), 0));
                            Broadcast(sess, $"pidx -1 1.{targetSession.Character.CharacterId}", ReceiverType.All);
                        }
                    }
                    ServerManager.Instance.Groups.Remove(grp);
                }

                session.Character.Group = null;
            }
        }

        //Server
        public bool Kick(string characterName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(characterName));
            if (session == null) return false;

            session.Client.Disconnect();
            return true;
        }

        //Map
        public void MapOut(long id)
        {
            foreach (ClientSession session in Sessions.Where(s => s.Character != null && s.Character.CharacterId == id))
            {
                session.Client.SendPacket(session.Character.GenerateMapOut());
                session.CurrentMap?.Broadcast(session, session.Character.GenerateOut(), ReceiverType.AllExceptMe);
            }
        }

        public void RequireBroadcastFromUser(ClientSession client, long characterId, string methodName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(characterId));
            if (session == null) return;

            MethodInfo method = session.Character.GetType().GetMethod(methodName);
            string result = (string)method.Invoke(session.Character, null);
            client.Client.SendPacket(result);
        }

        //Map
        public void ReviveFirstPosition(long characterId)
        {
            ClientSession Session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == characterId && s.Character.Hp <= 0);
            if (Session != null)
            {
                MapOut(Session.Character.CharacterId);
                Session.Character.MapId = 1;
                Session.Character.MapX = 80;
                Session.Character.MapY = 116;
                Session.Character.Hp = 1;
                Session.Character.Mp = 1;
                ChangeMap(Session.Character.CharacterId);
                Broadcast(Session, Session.Character.GenerateTp(), ReceiverType.All);
                Broadcast(Session, Session.Character.GenerateRevive(), ReceiverType.All);
                Session.Client.SendPacket(Session.Character.GenerateStat());
            }
        }

        public void SaveAll()
        {
            List<ClientSession> sessions = Sessions.Where(c => c.Client.CommunicationState == Core.Networking.Communication.Scs.Communication.CommunicationStates.Connected).ToList();
            sessions.ForEach(s => s.Character?.DeepCopy().Save());
        }

        public void SetProperty(long charId, string property, object value)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(charId));
            if (session == null) return;

            PropertyInfo propertyinfo = session.Character.GetType().GetProperties().Single(pi => pi.Name == property);
            propertyinfo.SetValue(session.Character, value, null);
        }

        //Server
        public void UpdateGroup(long charId)
        {
            Group myGroup = Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId));
            if (myGroup == null)
                return;
            string str = $"pinit { myGroup.Characters.Count()}";

            int i = 0;
            foreach (ClientSession session in Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId))?.Characters)
            {
                i++;
                str += $" 1|{session.Character.CharacterId}|{i}|{session.Character.Level}|{session.Character.Name}|11|{session.Character.Gender}|{session.Character.Class}|{(session.Character.UseSp ? session.Character.Morph : 0)}|{session.Character.HeroLevel}";
            }

            foreach (ClientSession session in myGroup.Characters)
            {
                session.Client.SendPacket(str);
            }
        }

        //Server
        private async void GroupProcess()
        {
            while (true)
            {
                foreach (Group grp in Groups)
                {
                    foreach (ClientSession session in grp.Characters)
                    {
                        foreach (string str in grp.GeneratePst())
                            session.Client.SendPacket(str);
                    }
                }
                await Task.Delay(2000);
            }
        }

        //Server
        private async void SaveAllProcess()
        {
            while (true)
            {
                await Task.Delay(60000 * 4);
                Logger.Log.Info(Language.Instance.GetMessageFromKey("SAVING_ALL"));
                SaveAll();
            }
        }

        //Map ??
        private async void TaskLauncherProcess()
        {
            Task TaskMap = null;
            while (true)
            {
                foreach (var GroupedSession in Sessions.Where(s => s.Character != null).GroupBy(s => s.Character.MapId))
                {
                    foreach (ClientSession Session in GroupedSession)
                    {
                        TaskMap = new Task(() => ServerManager.GetMap(Session.Character.MapId).MapTaskManager());
                        TaskMap.Start();
                    }
                }
                if (TaskMap != null)
                    await TaskMap;
                await Task.Delay(300);
            }
        }

        #endregion
    }
}