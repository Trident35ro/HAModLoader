using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HAModLoaderAPI
{
    public abstract class HAItem
    {
        public string name;

        public string overwrite_name;

        public Sprite inventory_sprite;

        public string crafting_desc;

        public string crafting_ingredientA;
    
        public int crafting_ingredientA_cnt;

        public string crafting_ingredientB;

        public int crafting_ingredientB_cnt;

        public inv_type_t type;

        public GameObject world_obj;

        public string crafting_IAP_key_required;

        public stacksize max_stack;

        public string equip_required_stat;

        public int equip_required_stat_lvl;

        public int market_cost;
    }
}
