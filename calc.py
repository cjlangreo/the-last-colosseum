def get_new_index(index: int, length: int) -> int:
    return index % length


for i in range(200):
    print(get_new_index(i,4))