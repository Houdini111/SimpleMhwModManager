namespace common
{
    public class Armor
    {
        public enum ARMOR_SLOT { HEAD, CHEST, ARMS, WAIST, LEGS }

        public ARMOR_SLOT type { get; set; }
        public int armor_ID { get; set; }
        public string name { get; set; }
        public int layered_ID { get; set; }
        public string male_location { get; set; }
        public string female_location { get; set; }
    }

    public class Weapon
    {
        //As we call them, BOW, CB, GL, HAMMER, HBG, LANCE, LS, LBG, IG, SA, SS, LS, HH, DB 
        public enum WEAPON_TYPE { BOW, C_AXE, G_LANCE, HAMMER, HBG, LANCE, L_SWORD, LBG, ROD, S_AXE, SWORD, TACHI, WHISTLE, W_SWORD }
        public enum MODEL_TYPE { COMBINED, INDEPENDENT }

        public WEAPON_TYPE weapon_type { get; set; }
        public int ID { get; set; }
        public string name { get; set; }
        public MODEL_TYPE model_type { get; set; }
        public string main_model { get; set; }
        public string part_model { get; set; }
    }
}
