namespace TestApp
{
    public static class Helper
    {
        public static Result And(this Result r1, Result r2)
        {
            if (r1 && r2) return Result.SUCCESS;
            if ((!r1) && (!r2)) return Result.Fail( r1.Message + "\r\nand\r\n" + r2.Message);
            if (!r1) return r1;
            return r2;
        }
    }
}
