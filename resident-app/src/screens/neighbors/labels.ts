// Маппинги для int-enum'ов из бэка → русские лейблы и обратные ключи.
// Backend возвращает int-индексы в DTO (см. ExpenseDto.categoryId, ItemDto.unit и т.п.),
// но принимает либо int, либо string-имена в командах. Мы шлём имена для читаемости.

export const EXPENSE_CATEGORY_NAMES = [
  'Groceries',
  'Rent',
  'Utilities',
  'Internet',
  'Household',
  'Other',
] as const;
export const EXPENSE_CATEGORY_LABEL: Record<number, string> = {
  0: 'Продукты',
  1: 'Аренда',
  2: 'Коммуналка',
  3: 'Интернет',
  4: 'Хозтовары',
  5: 'Другое',
};

export const ITEM_STATUS_NAMES = ['Available', 'RunningLow', 'InCart', 'Consumed'] as const;
export const ITEM_CATEGORY_LABEL: Record<number, string> = {
  0: 'Еда',
  1: 'Хозтовары',
  2: 'Гигиена',
  3: 'Другое',
};
export const STORAGE_LOCATION_LABEL: Record<number, string> = {
  0: 'Холодильник',
  1: 'Морозилка',
  2: 'Полка',
  3: 'Ванная',
  4: 'Другое',
};
export const UNIT_LABEL: Record<number, string> = {
  0: 'шт',
  1: 'упак',
  2: 'бут',
  3: 'кг',
  4: 'г',
  5: 'л',
  6: 'мл',
};

export const CHORE_CATEGORY_LABEL: Record<number, string> = {
  0: 'Пылесос',
  1: 'Полы',
  2: 'Ванная',
  3: 'Кухня',
  4: 'Посуда',
  5: 'Мусор',
  6: 'Другое',
};
export const CHORE_STATUS_LABEL: Record<number, string> = {
  0: 'Активна',
  1: 'На проверке',
  2: 'Готово',
};
