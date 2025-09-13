using UnityEngine;

public class UIElementChanged : MonoBehaviour
{

   private ConwaysGameOfLife gameOfLife;
   
   private void Start()
   {
      gameOfLife = GetComponent<ConwaysGameOfLife>();
   }

   public void UpdateRowCount(FloatEvent ctx)
   {
      gameOfLife.Rows = (int)ctx.FloatValue;
   }
   public void UpdateColumnCount(FloatEvent ctx)
   {
      gameOfLife.Columns = (int)ctx.FloatValue;
   }
   public void UpdatePlayGame(BoolEvent ctx)
   {
      gameOfLife.PauseGame = ctx.Value;
   }
   public void UpdatePlaySpeed(FloatEvent ctx)
   {
      gameOfLife.PlaySpeed = ctx.FloatValue;
   }
}
