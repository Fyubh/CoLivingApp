import { Fragment, useCallback, useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Modal,
  Pressable,
  RefreshControl,
  ScrollView,
  Text,
  TextInput,
  View,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { api } from '../../api/client';
import {
  ItemCategoryInt,
  ItemDto,
  ItemStatusInt,
  ItemUnitInt,
  MyApartmentContextDto,
  StorageLocationInt,
} from '../../api/types';
import {
  Button,
  Divider,
  Glass,
  Icon,
  IconName,
  Pill,
  SectionLabel,
  Segmented,
} from '../../components';
import { useSession } from '../../state/SessionContext';
import { RADIUS, T, TYPE } from '../../theme/tokens';
import {
  ITEM_CATEGORY_LABEL,
  STORAGE_LOCATION_LABEL,
  UNIT_LABEL,
} from './labels';

const SUB_TABS = ['Холодильник', 'Список покупок'] as const;
type SubTab = (typeof SUB_TABS)[number];

export function ProductsTab({ context }: { context: MyApartmentContextDto }) {
  const { token } = useSession();
  const [sub, setSub] = useState<SubTab>('Холодильник');
  const [stock, setStock] = useState<ItemDto[]>([]);
  const [cart, setCart] = useState<ItemDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showAdd, setShowAdd] = useState(false);
  const [busy, setBusy] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    try {
      const [s, c] = await Promise.all([
        api.getItems(token, context.apartmentId, 0), // Available
        api.getItems(token, context.apartmentId, 2), // InCart
      ]);
      setStock(s);
      setCart(c);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setLoading(false);
    }
  }, [token, context.apartmentId]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  async function moveToCart(itemId: string) {
    if (!token) return;
    setBusy(itemId);
    try {
      await api.moveItemToCart(token, itemId, context.apartmentId);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setBusy(null);
    }
  }

  async function consume(itemId: string) {
    if (!token) return;
    setBusy(itemId);
    try {
      await api.consumeItem(token, itemId, context.apartmentId);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setBusy(null);
    }
  }

  async function removeItem(itemId: string) {
    if (!token) return;
    setBusy(itemId);
    try {
      await api.removeItem(token, itemId, context.apartmentId);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setBusy(null);
    }
  }

  const totalCart = useMemo(
    () => cart.reduce((acc) => acc + 0, 0), // backend не отдаёт price; checkout попросит сумму у юзера
    [cart]
  );

  async function checkout() {
    if (!token || cart.length === 0) return;
    Alert.prompt?.(
      'Оплата корзины',
      'Сколько потратили?',
      async (val) => {
        if (!val) return;
        const total = Number(String(val).replace(',', '.'));
        if (!Number.isFinite(total) || total <= 0) {
          setError('Введите сумму больше 0');
          return;
        }
        setBusy('checkout');
        try {
          await api.checkout(token, {
            apartmentId: context.apartmentId,
            totalAmount: total,
            itemIds: cart.map((c) => c.id),
          });
          await refresh();
        } catch (e) {
          setError(e instanceof Error ? e.message : 'Ошибка');
        } finally {
          setBusy(null);
        }
      },
      'plain-text',
      '',
      'decimal-pad'
    );
  }

  const items = sub === 'Холодильник' ? stock : cart;
  const empty = items.length === 0;

  return (
    <ScrollView
      contentContainerStyle={{ padding: 16, paddingBottom: 140 }}
      refreshControl={<RefreshControl refreshing={loading} onRefresh={refresh} />}
    >
      <Segmented options={SUB_TABS} value={sub} onChange={setSub} />

      <View style={{ marginTop: 16 }}>
        <SectionLabel
          trailing={
            <Pressable onPress={() => setShowAdd(true)}>
              <Text style={[TYPE.footMed, { color: T.accent }]}>+ Добавить</Text>
            </Pressable>
          }
        >
          {sub === 'Холодильник' ? 'Запасы' : 'В корзине'}
        </SectionLabel>
        {empty ? (
          <Glass padding={20} radius={20}>
            <Text style={[TYPE.body, { color: T.text2, textAlign: 'center' }]}>
              {sub === 'Холодильник'
                ? 'Холодильник пуст. Добавьте первый продукт.'
                : 'В списке покупок ничего нет.'}
            </Text>
          </Glass>
        ) : (
          <Glass padding={0} radius={20}>
            {items.map((item, i) => (
              <Fragment key={item.id}>
                <ItemRow
                  item={item}
                  inCart={sub === 'Список покупок'}
                  busy={busy === item.id}
                  onMove={() => moveToCart(item.id)}
                  onConsume={() => consume(item.id)}
                  onRemove={() => removeItem(item.id)}
                />
                {i < items.length - 1 && <Divider inset={68} />}
              </Fragment>
            ))}
          </Glass>
        )}
      </View>

      {sub === 'Список покупок' && cart.length > 0 && (
        <View style={{ marginTop: 14 }}>
          <Button
            kind="primary"
            full
            leading={<Icon name="split" size={18} color="#fff" />}
            onPress={checkout}
            disabled={busy === 'checkout'}
          >
            {busy === 'checkout' ? 'Делим...' : 'Купить и поделить счёт'}
          </Button>
        </View>
      )}

      {error && (
        <View style={{ marginTop: 16 }}>
          <Glass padding={14} radius={16}>
            <Text style={[TYPE.footnote, { color: T.danger }]}>{error}</Text>
          </Glass>
        </View>
      )}

      <AddItemModal
        visible={showAdd}
        onClose={() => setShowAdd(false)}
        context={context}
        defaultStatus={sub === 'Холодильник' ? 0 : 2}
        onDone={() => {
          setShowAdd(false);
          void refresh();
        }}
      />
    </ScrollView>
  );
}

function ItemRow({
  item,
  inCart,
  busy,
  onMove,
  onConsume,
  onRemove,
}: {
  item: ItemDto;
  inCart: boolean;
  busy: boolean;
  onMove: () => void;
  onConsume: () => void;
  onRemove: () => void;
}) {
  const expiry = item.expiryDate ? new Date(item.expiryDate) : null;
  const expired = expiry && expiry.getTime() < Date.now();
  const soon = expiry && !expired && expiry.getTime() - Date.now() < 1000 * 60 * 60 * 24 * 3;
  const locName = STORAGE_LOCATION_LABEL[item.location] ?? '—';
  const isFridge = item.location === 0;
  const iconName: IconName = isFridge ? 'fridge' : 'shelf';
  const iconColor = isFridge ? T.accent : T.violet;
  return (
    <View style={{ paddingHorizontal: 14, paddingVertical: 12 }}>
      <View style={{ flexDirection: 'row', alignItems: 'center', gap: 12 }}>
        <View
          style={{
            width: 38,
            height: 38,
            borderRadius: 12,
            backgroundColor:
              isFridge ? 'rgba(31,107,255,0.10)' : 'rgba(125,107,255,0.10)',
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          <Icon name={iconName} size={20} color={iconColor} />
        </View>
        <View style={{ flex: 1 }}>
          <Text style={[TYPE.bodyMed, { color: T.text }]} numberOfLines={1}>
            {item.name}
          </Text>
          <Text style={[TYPE.caption, { color: T.text2, marginTop: 1 }]}>
            {locName} · {item.quantity} {UNIT_LABEL[item.unit] ?? ''} ·{' '}
            {ITEM_CATEGORY_LABEL[item.category] ?? ''}
          </Text>
        </View>
        {expiry && (
          <Text
            style={[
              TYPE.caption,
              {
                color: expired ? T.danger : soon ? T.warning : T.text2,
                fontWeight: '600',
              },
            ]}
          >
            {expiry.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })}
          </Text>
        )}
      </View>
      <View style={{ flexDirection: 'row', gap: 8, marginTop: 10, marginLeft: 50 }}>
        {!inCart && (
          <Button kind="secondary" size="sm" onPress={onMove} disabled={busy}>
            В корзину
          </Button>
        )}
        {!inCart && (
          <Button kind="glass" size="sm" onPress={onConsume} disabled={busy}>
            Съели
          </Button>
        )}
        <Button
          kind="glass"
          size="sm"
          onPress={onRemove}
          disabled={busy}
          textColor={T.danger}
        >
          Удалить
        </Button>
      </View>
    </View>
  );
}

const CATEGORY_OPTIONS: { v: ItemCategoryInt; label: string }[] = [
  { v: 0, label: 'Еда' },
  { v: 1, label: 'Хозтовары' },
  { v: 2, label: 'Гигиена' },
  { v: 3, label: 'Другое' },
];
const LOCATION_OPTIONS: { v: StorageLocationInt; label: string }[] = [
  { v: 0, label: 'Холод-к' },
  { v: 1, label: 'Морозилка' },
  { v: 2, label: 'Полка' },
  { v: 3, label: 'Ванная' },
  { v: 4, label: 'Другое' },
];
const UNIT_OPTIONS: { v: ItemUnitInt; label: string }[] = [
  { v: 0, label: 'шт' },
  { v: 1, label: 'упак' },
  { v: 2, label: 'бут' },
  { v: 3, label: 'кг' },
  { v: 4, label: 'г' },
  { v: 5, label: 'л' },
  { v: 6, label: 'мл' },
];

function AddItemModal({
  visible,
  onClose,
  context,
  defaultStatus,
  onDone,
}: {
  visible: boolean;
  onClose: () => void;
  context: MyApartmentContextDto;
  defaultStatus: ItemStatusInt;
  onDone: () => void;
}) {
  const insets = useSafeAreaInsets();
  const { token } = useSession();
  const [name, setName] = useState('');
  const [quantity, setQuantity] = useState('1');
  const [unit, setUnit] = useState<ItemUnitInt>(0);
  const [category, setCategory] = useState<ItemCategoryInt>(0);
  const [location, setLocation] = useState<StorageLocationInt>(0);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (visible) {
      setName('');
      setQuantity('1');
      setUnit(0);
      setCategory(0);
      setLocation(0);
      setError(null);
    }
  }, [visible]);

  async function submit() {
    if (!token) return;
    if (!name.trim()) {
      setError('Введите название');
      return;
    }
    const qty = Number(quantity.replace(',', '.'));
    if (!Number.isFinite(qty) || qty <= 0) {
      setError('Количество должно быть > 0');
      return;
    }
    setBusy(true);
    try {
      await api.createItem(token, {
        apartmentId: context.apartmentId,
        customName: name.trim(),
        quantity: qty,
        unit,
        status: defaultStatus,
        category,
        location,
        expiryDate: null,
      });
      onDone();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal
      visible={visible}
      animationType="slide"
      presentationStyle="pageSheet"
      onRequestClose={onClose}
    >
      <View style={{ flex: 1, backgroundColor: T.bg }}>
        <View
          style={{
            paddingTop: insets.top + 8,
            paddingBottom: 12,
            paddingHorizontal: 20,
            flexDirection: 'row',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottomColor: T.divider,
            borderBottomWidth: 0.5,
          }}
        >
          <Pressable onPress={onClose}>
            <Text style={[TYPE.bodyMed, { color: T.accent }]}>Отмена</Text>
          </Pressable>
          <Text style={[TYPE.headline, { color: T.text }]}>Новый продукт</Text>
          <Pressable onPress={submit} disabled={busy}>
            <Text
              style={[
                TYPE.bodyMed,
                { color: busy ? T.text3 : T.accent, fontWeight: '700' },
              ]}
            >
              {busy ? '...' : 'Добавить'}
            </Text>
          </Pressable>
        </View>
        <ScrollView contentContainerStyle={{ padding: 16, gap: 16 }}>
          <FieldBlock label="Название">
            <Input value={name} onChangeText={setName} placeholder="Например: молоко" />
          </FieldBlock>
          <View style={{ flexDirection: 'row', gap: 12 }}>
            <View style={{ flex: 1 }}>
              <FieldBlock label="Количество">
                <Input
                  value={quantity}
                  onChangeText={setQuantity}
                  keyboardType="decimal-pad"
                />
              </FieldBlock>
            </View>
            <View style={{ flex: 1 }}>
              <FieldBlock label="Единица">
                <ChipPicker value={unit} options={UNIT_OPTIONS} onChange={setUnit} />
              </FieldBlock>
            </View>
          </View>
          <FieldBlock label="Категория">
            <ChipPicker value={category} options={CATEGORY_OPTIONS} onChange={setCategory} />
          </FieldBlock>
          <FieldBlock label="Где хранить">
            <ChipPicker value={location} options={LOCATION_OPTIONS} onChange={setLocation} />
          </FieldBlock>
          {error && (
            <Pill color={T.danger} soft={T.dangerSoft}>{error}</Pill>
          )}
        </ScrollView>
      </View>
    </Modal>
  );
}

function FieldBlock({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <View>
      <Text style={[TYPE.footMed, { color: T.text2, marginBottom: 6, paddingLeft: 4 }]}>
        {label}
      </Text>
      {children}
    </View>
  );
}

function Input(props: React.ComponentProps<typeof TextInput>) {
  return (
    <View
      style={{
        backgroundColor: '#fff',
        borderColor: T.border,
        borderWidth: 1,
        borderRadius: RADIUS.input,
        paddingHorizontal: 14,
        height: 50,
        justifyContent: 'center',
      }}
    >
      <TextInput
        placeholderTextColor={T.text3}
        style={{ fontSize: 15, color: T.text, padding: 0 }}
        {...props}
      />
    </View>
  );
}

function ChipPicker<V extends number>({
  value,
  options,
  onChange,
}: {
  value: V;
  options: { v: V; label: string }[];
  onChange: (v: V) => void;
}) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={{ gap: 8, paddingHorizontal: 4 }}
    >
      {options.map((o) => {
        const active = o.v === value;
        return (
          <Pressable
            key={o.v}
            onPress={() => onChange(o.v)}
            style={{
              paddingHorizontal: 14,
              paddingVertical: 8,
              borderRadius: 999,
              backgroundColor: active ? T.accent : '#fff',
              borderColor: active ? T.accent : T.border,
              borderWidth: 1,
            }}
          >
            <Text
              style={[
                TYPE.footMed,
                { color: active ? '#fff' : T.text, fontWeight: '700' },
              ]}
            >
              {o.label}
            </Text>
          </Pressable>
        );
      })}
    </ScrollView>
  );
}
