public interface IEnemyTargetSelector
{
    CardInfoScript SelectRandomEnemy();
    CardInfoScript SelectRandomAlly(CardInfoScript exclude = null);

    CardInfoScript SelectEnemyByPoints(int maxPoints);
}
