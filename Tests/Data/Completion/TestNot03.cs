// ${COMPLETE_ITEM:not}

class Foo
{
  void Bar(int a, bool b, int[] xs)
  {
    !b.{caret}
    (a < 0 ? xs : xs)[0] = 42;
  }
}