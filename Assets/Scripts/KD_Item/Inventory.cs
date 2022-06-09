using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public delegate void OnSlotCountChange(int val);
    public OnSlotCountChange onSlotCountChange;


    public delegate void OnChnageItem();
    public OnChnageItem onChnageItem;

    public List<Item> items = new List<Item>();

    public int slotCount;
    public int SlotCnt
    {
        get => slotCount;
        set
        {
            slotCount = value;
            onSlotCountChange.Invoke(slotCount);
        }
    }

    #region 싱글턴
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
            return;
        }
    }
    #endregion

    private void Start()
    {
        slotCount = 5;
    }

    //items라는 이름의 Item의 리스트 사용하며
    //items의 개수가 slotCnt (현재 활성 슬롯)보다 작을때만 
    //활성화되도록. 

    //아이템 추가에 성공할시 true가 뜨고 실패시 false가 뜬다. 
    public bool AddItem(Item _item)
    {
        if (items.Count < slotCount)
        {
            items.Add(_item);
            if (onChnageItem != null)
            {
                onChnageItem.Invoke();
                return true;
                Debug.Log("additem true");
            }
        }

        Debug.Log("additem false");
        return false;
    }


    //플레이어가 fielditem과 닿았을시ㅡ 
    //필드 아이템을 파괴해준다. 
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Item"))
        {
            Debug.Log("아이템에 닿았다"); //현재 이거 자체가 씹히고 있는 상황. 
            FiedItems fiedItems = col.GetComponent<FiedItems>();
            if (AddItem(fiedItems.GetItem()))
            {
                fiedItems.DestoryItem();
            }
        }
    }


    /// <summary>
    /// 대리자 정의, 대리자 인스턴스 화
    /// set안에서 대리자 호출한다. 
    /// </summary>


}
