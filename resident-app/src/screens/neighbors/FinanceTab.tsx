import { Fragment, useCallback, useEffect, useMemo, useState } from 'react';
import {
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
  ExpenseDto,
  MyApartmentContextDto,
  RecurringExpenseDto,
  SettlementDto,
  UserBalanceDto,
} from '../../api/types';
import {
  Avatar,
  Button,
  Divider,
  Glass,
  Icon,
  Pill,
  SectionLabel,
} from '../../components';
import { useSession } from '../../state/SessionContext';
import { RADIUS, T, TYPE } from '../../theme/tokens';
import { EXPENSE_CATEGORY_LABEL } from './labels';

export function FinanceTab({ context }: { context: MyApartmentContextDto }) {
  const { token } = useSession();
  const [balances, setBalances] = useState<UserBalanceDto[]>([]);
  const [recurring, setRecurring] = useState<RecurringExpenseDto[]>([]);
  const [settlements, setSettlements] = useState<SettlementDto[]>([]);
  const [expenses, setExpenses] = useState<ExpenseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showSettle, setShowSettle] = useState(false);

  const refresh = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    try {
      const [b, r, s, e] = await Promise.all([
        api.getBalances(token, context.apartmentId),
        api.getRecurring(token, context.apartmentId),
        api.getSettlements(token, context.apartmentId),
        api.getExpenses(token, context.apartmentId),
      ]);
      setBalances(b);
      setRecurring(r);
      setSettlements(s);
      setExpenses(e);
    } catch (ex) {
      setError(ex instanceof Error ? ex.message : 'Ошибка');
    } finally {
      setLoading(false);
    }
  }, [token, context.apartmentId]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const others = useMemo(
    () => context.roommates.filter((r) => !r.isMe),
    [context.roommates]
  );
  const meBalance = balances.find((b) => b.userId === context.meUserId)?.balance ?? 0;

  return (
    <ScrollView
      contentContainerStyle={{ padding: 16, paddingBottom: 140 }}
      refreshControl={<RefreshControl refreshing={loading} onRefresh={refresh} />}
    >
      <Glass padding={18} radius={22}>
        <Text style={[TYPE.footnote, { color: T.text2, textAlign: 'center' }]}>
          Ваш баланс по квартире
        </Text>
        <Text
          style={[
            TYPE.display,
            {
              color: meBalance < 0 ? T.danger : T.success,
              textAlign: 'center',
              marginTop: 6,
            },
          ]}
        >
          {meBalance < 0 ? '-' : '+'}${Math.abs(meBalance).toFixed(2)}
        </Text>
        <Text
          style={[TYPE.caption, { color: T.text2, textAlign: 'center', marginTop: 2 }]}
        >
          {meBalance < 0 ? 'вы должны' : meBalance > 0 ? 'вам должны' : 'все сходится'}
        </Text>

        {balances.length > 1 && (
          <View style={{ marginTop: 14, gap: 10 }}>
            {balances
              .filter((b) => b.userId !== context.meUserId)
              .map((b) => (
                <BalanceRow key={b.userId} balance={b} />
              ))}
          </View>
        )}

        {others.length > 0 && (
          <View style={{ marginTop: 14, flexDirection: 'row', gap: 8 }}>
            <Button
              kind="primary"
              size="sm"
              full
              style={{ flex: 1 }}
              onPress={() => setShowSettle(true)}
            >
              Вернуть долг
            </Button>
          </View>
        )}
      </Glass>

      {recurring.length > 0 && (
        <View style={{ marginTop: 18 }}>
          <SectionLabel>Регулярные платежи</SectionLabel>
          <Glass padding={0} radius={20}>
            {recurring.map((r, i) => (
              <Fragment key={r.id}>
                <RecurringRow item={r} />
                {i < recurring.length - 1 && <Divider inset={56} />}
              </Fragment>
            ))}
          </Glass>
        </View>
      )}

      {settlements.length > 0 && (
        <View style={{ marginTop: 18 }}>
          <SectionLabel>История переводов</SectionLabel>
          <Glass padding={0} radius={20}>
            {settlements.slice(0, 8).map((s, i) => (
              <Fragment key={s.id}>
                <SettlementRow item={s} />
                {i < Math.min(7, settlements.length - 1) && <Divider inset={56} />}
              </Fragment>
            ))}
          </Glass>
        </View>
      )}

      {expenses.length > 0 && (
        <View style={{ marginTop: 18 }}>
          <SectionLabel>Общие траты</SectionLabel>
          <Glass padding={0} radius={20}>
            {expenses.slice(0, 10).map((e, i) => (
              <Fragment key={e.id}>
                <ExpenseRow item={e} />
                {i < Math.min(9, expenses.length - 1) && <Divider inset={56} />}
              </Fragment>
            ))}
          </Glass>
        </View>
      )}

      {error && (
        <View style={{ marginTop: 16 }}>
          <Glass padding={14} radius={16}>
            <Text style={[TYPE.footnote, { color: T.danger }]}>{error}</Text>
          </Glass>
        </View>
      )}

      <SettleModal
        visible={showSettle}
        onClose={() => setShowSettle(false)}
        context={context}
        onDone={() => {
          setShowSettle(false);
          void refresh();
        }}
      />
    </ScrollView>
  );
}

function BalanceRow({ balance }: { balance: UserBalanceDto }) {
  const owed = balance.balance > 0; // им должны нам? нет — баланс другого: положительный = они должны нам
  const isPositive = balance.balance >= 0;
  return (
    <View style={{ flexDirection: 'row', alignItems: 'center', gap: 12 }}>
      <Avatar
        initial={balance.userName.charAt(0)}
        size={32}
        color={isPositive ? T.success : T.violet}
      />
      <Text style={[TYPE.body, { color: T.text, flex: 1 }]} numberOfLines={1}>
        {balance.userName}
      </Text>
      <Text
        style={[
          TYPE.bodyMed,
          { color: isPositive ? T.success : T.danger },
        ]}
      >
        {isPositive ? 'должен ' : 'вы должны '}${Math.abs(balance.balance).toFixed(2)}
      </Text>
    </View>
  );
}

function RecurringRow({ item }: { item: RecurringExpenseDto }) {
  const next = new Date(item.nextRunDate);
  return (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        gap: 12,
        paddingHorizontal: 14,
        paddingVertical: 12,
      }}
    >
      <View
        style={{
          width: 38,
          height: 38,
          borderRadius: 12,
          backgroundColor: T.accentSoft,
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Icon name="refresh" size={20} color={T.accent} />
      </View>
      <View style={{ flex: 1 }}>
        <Text style={[TYPE.bodyMed, { color: T.text }]} numberOfLines={1}>
          {item.description}
        </Text>
        <Text style={[TYPE.footnote, { color: T.text2, marginTop: 1 }]}>
          след. {next.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })} ·{' '}
          {item.payerName}
        </Text>
      </View>
      <Text style={[TYPE.bodyMed, { color: T.text }]}>${item.amount.toFixed(2)}</Text>
    </View>
  );
}

function SettlementRow({ item }: { item: SettlementDto }) {
  const date = new Date(item.date);
  return (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        gap: 12,
        paddingHorizontal: 14,
        paddingVertical: 12,
      }}
    >
      <View
        style={{
          width: 32,
          height: 32,
          borderRadius: 16,
          backgroundColor: T.successSoft,
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Icon name="arrow-right" size={16} color={T.success} />
      </View>
      <View style={{ flex: 1 }}>
        <Text style={[TYPE.body, { color: T.text }]} numberOfLines={1}>
          <Text style={{ fontWeight: '600' }}>{item.senderName}</Text>
          {' → '}
          <Text style={{ fontWeight: '600' }}>{item.receiverName}</Text>
        </Text>
        <Text style={[TYPE.footnote, { color: T.text2, marginTop: 1 }]}>
          {date.toLocaleDateString('ru-RU')}
        </Text>
      </View>
      <Text style={[TYPE.bodyMed, { color: T.text }]}>${item.amount.toFixed(2)}</Text>
    </View>
  );
}

function ExpenseRow({ item }: { item: ExpenseDto }) {
  return (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        gap: 12,
        paddingHorizontal: 14,
        paddingVertical: 12,
      }}
    >
      <View
        style={{
          width: 38,
          height: 38,
          borderRadius: 12,
          backgroundColor: 'rgba(125,107,255,0.12)',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Icon name="shopping" size={20} color={T.violet} />
      </View>
      <View style={{ flex: 1 }}>
        <Text style={[TYPE.bodyMed, { color: T.text }]} numberOfLines={1}>
          {item.description}
        </Text>
        <Text style={[TYPE.footnote, { color: T.text2, marginTop: 1 }]} numberOfLines={1}>
          {item.payerName} · {EXPENSE_CATEGORY_LABEL[item.categoryId] ?? '—'}
        </Text>
      </View>
      <Text style={[TYPE.bodyMed, { color: T.text }]}>${item.amount.toFixed(2)}</Text>
    </View>
  );
}

function SettleModal({
  visible,
  onClose,
  context,
  onDone,
}: {
  visible: boolean;
  onClose: () => void;
  context: MyApartmentContextDto;
  onDone: () => void;
}) {
  const insets = useSafeAreaInsets();
  const { token } = useSession();
  const others = context.roommates.filter((r) => !r.isMe);
  const [receiverId, setReceiverId] = useState<string>(others[0]?.userId ?? '');
  const [amount, setAmount] = useState<string>('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (visible) {
      setReceiverId(others[0]?.userId ?? '');
      setAmount('');
      setError(null);
    }
  }, [visible, others]);

  async function submit() {
    if (!token) return;
    const amt = Number(amount.replace(',', '.'));
    if (!Number.isFinite(amt) || amt <= 0) {
      setError('Введите сумму больше 0');
      return;
    }
    if (!receiverId) {
      setError('Выберите получателя');
      return;
    }
    setBusy(true);
    try {
      await api.settleDebt(token, {
        apartmentId: context.apartmentId,
        receiverId,
        amount: amt,
      });
      onDone();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal visible={visible} animationType="slide" presentationStyle="pageSheet" onRequestClose={onClose}>
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
          <Text style={[TYPE.headline, { color: T.text }]}>Вернуть долг</Text>
          <Pressable onPress={submit} disabled={busy}>
            <Text
              style={[
                TYPE.bodyMed,
                { color: busy ? T.text3 : T.accent, fontWeight: '700' },
              ]}
            >
              {busy ? '...' : 'Готово'}
            </Text>
          </Pressable>
        </View>
        <ScrollView contentContainerStyle={{ padding: 16, gap: 16 }}>
          <View>
            <Text style={[TYPE.footMed, { color: T.text2, marginBottom: 8, paddingLeft: 4 }]}>
              Кому
            </Text>
            <View style={{ gap: 8 }}>
              {others.map((r) => (
                <Pressable
                  key={r.userId}
                  onPress={() => setReceiverId(r.userId)}
                  style={{
                    flexDirection: 'row',
                    alignItems: 'center',
                    gap: 12,
                    padding: 12,
                    backgroundColor: receiverId === r.userId ? T.accentSoft : '#fff',
                    borderColor: receiverId === r.userId ? T.accent : T.border,
                    borderWidth: 1,
                    borderRadius: RADIUS.input,
                  }}
                >
                  <Avatar initial={r.name.charAt(0)} size={32} color={T.violet} />
                  <Text style={[TYPE.body, { color: T.text, flex: 1 }]}>{r.name}</Text>
                  {receiverId === r.userId && (
                    <Icon name="check" size={18} color={T.accent} />
                  )}
                </Pressable>
              ))}
            </View>
          </View>
          <View>
            <Text style={[TYPE.footMed, { color: T.text2, marginBottom: 8, paddingLeft: 4 }]}>
              Сумма ($)
            </Text>
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
                value={amount}
                onChangeText={setAmount}
                placeholder="0.00"
                placeholderTextColor={T.text3}
                keyboardType="decimal-pad"
                style={{ fontSize: 17, color: T.text, padding: 0 }}
              />
            </View>
          </View>
          {error && (
            <Pill color={T.danger} soft={T.dangerSoft}>{error}</Pill>
          )}
        </ScrollView>
      </View>
    </Modal>
  );
}
