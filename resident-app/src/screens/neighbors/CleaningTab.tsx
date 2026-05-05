import { Fragment, useCallback, useEffect, useState } from 'react';
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
  ChoreCategoryInt,
  ChoreDto,
  MyApartmentContextDto,
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
import { CHORE_CATEGORY_LABEL, CHORE_STATUS_LABEL } from './labels';

export function CleaningTab({ context }: { context: MyApartmentContextDto }) {
  const { token } = useSession();
  const [chores, setChores] = useState<ChoreDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState<string | null>(null);
  const [showAdd, setShowAdd] = useState(false);

  const refresh = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    try {
      setChores(await api.getChores(token, context.apartmentId));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setLoading(false);
    }
  }, [token, context.apartmentId]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  async function complete(id: string) {
    if (!token) return;
    setBusy(id);
    try {
      await api.completeChore(token, id, context.apartmentId);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setBusy(null);
    }
  }
  async function confirm(id: string) {
    if (!token) return;
    setBusy(id);
    try {
      await api.confirmChore(token, id, context.apartmentId);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setBusy(null);
    }
  }
  async function reject(id: string) {
    if (!token) return;
    setBusy(id);
    try {
      await api.rejectChore(token, id, context.apartmentId);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setBusy(null);
    }
  }

  const active = chores.filter((c) => c.status !== 2);
  const done = chores.filter((c) => c.status === 2);

  return (
    <ScrollView
      contentContainerStyle={{ padding: 16, paddingBottom: 140 }}
      refreshControl={<RefreshControl refreshing={loading} onRefresh={refresh} />}
    >
      <View>
        <SectionLabel
          trailing={
            <Pressable onPress={() => setShowAdd(true)}>
              <Text style={[TYPE.footMed, { color: T.accent }]}>+ Задача</Text>
            </Pressable>
          }
        >
          Активные
        </SectionLabel>
        {active.length === 0 ? (
          <Glass padding={20} radius={20}>
            <Text style={[TYPE.body, { color: T.text2, textAlign: 'center' }]}>
              Активных задач нет.
            </Text>
          </Glass>
        ) : (
          <Glass padding={0} radius={20}>
            {active.map((c, i) => (
              <Fragment key={c.id}>
                <ChoreRow
                  chore={c}
                  busy={busy === c.id}
                  onComplete={() => complete(c.id)}
                  onConfirm={() => confirm(c.id)}
                  onReject={() => reject(c.id)}
                />
                {i < active.length - 1 && <Divider inset={68} />}
              </Fragment>
            ))}
          </Glass>
        )}
      </View>

      {done.length > 0 && (
        <View style={{ marginTop: 18 }}>
          <SectionLabel>История</SectionLabel>
          <Glass padding={0} radius={20}>
            {done.slice(0, 6).map((c, i) => (
              <Fragment key={c.id}>
                <ChoreRow
                  chore={c}
                  busy={false}
                  onComplete={() => {}}
                  onConfirm={() => {}}
                  onReject={() => {}}
                />
                {i < Math.min(5, done.length - 1) && <Divider inset={68} />}
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

      <AddChoreModal
        visible={showAdd}
        onClose={() => setShowAdd(false)}
        context={context}
        onDone={() => {
          setShowAdd(false);
          void refresh();
        }}
      />
    </ScrollView>
  );
}

function ChoreRow({
  chore,
  busy,
  onComplete,
  onConfirm,
  onReject,
}: {
  chore: ChoreDto;
  busy: boolean;
  onComplete: () => void;
  onConfirm: () => void;
  onReject: () => void;
}) {
  const due = chore.dueDate ? new Date(chore.dueDate) : null;
  const overdue = due && due.getTime() < Date.now() && chore.status === 0;
  const tone =
    chore.status === 2
      ? T.success
      : chore.status === 1
        ? T.warning
        : overdue
          ? T.danger
          : T.text2;
  const toneSoft =
    chore.status === 2
      ? T.successSoft
      : chore.status === 1
        ? T.warningSoft
        : overdue
          ? T.dangerSoft
          : 'rgba(20,24,40,0.06)';
  return (
    <View style={{ paddingHorizontal: 14, paddingVertical: 12 }}>
      <View style={{ flexDirection: 'row', alignItems: 'center', gap: 12 }}>
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
          <Icon name="broom" size={20} color={T.accent} />
        </View>
        <View style={{ flex: 1 }}>
          <Text style={[TYPE.bodyMed, { color: T.text }]} numberOfLines={1}>
            {chore.title}
          </Text>
          <Text style={[TYPE.caption, { color: T.text2, marginTop: 1 }]} numberOfLines={1}>
            {chore.assignedName ?? 'без исполнителя'} ·{' '}
            {CHORE_CATEGORY_LABEL[chore.category] ?? ''}
            {due
              ? ` · до ${due.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })}`
              : ''}
          </Text>
        </View>
        <Pill color={tone} soft={toneSoft}>
          {CHORE_STATUS_LABEL[chore.status] ?? '—'}
        </Pill>
      </View>

      {chore.canComplete && chore.status === 0 && (
        <View style={{ flexDirection: 'row', gap: 8, marginTop: 10, marginLeft: 50 }}>
          <Button kind="success" size="sm" onPress={onComplete} disabled={busy}>
            Готово
          </Button>
        </View>
      )}
      {chore.canReview && chore.status === 1 && (
        <View style={{ flexDirection: 'row', gap: 8, marginTop: 10, marginLeft: 50 }}>
          <Button kind="success" size="sm" onPress={onConfirm} disabled={busy}>
            Принять
          </Button>
          <Button
            kind="glass"
            size="sm"
            onPress={onReject}
            disabled={busy}
            textColor={T.danger}
          >
            Отклонить
          </Button>
        </View>
      )}
    </View>
  );
}

const CATEGORY_OPTIONS: { v: ChoreCategoryInt; label: string }[] = [
  { v: 0, label: 'Пылесос' },
  { v: 1, label: 'Полы' },
  { v: 2, label: 'Ванная' },
  { v: 3, label: 'Кухня' },
  { v: 4, label: 'Посуда' },
  { v: 5, label: 'Мусор' },
  { v: 6, label: 'Другое' },
];

function AddChoreModal({
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
  const [title, setTitle] = useState('');
  const [category, setCategory] = useState<ChoreCategoryInt>(0);
  const [assignee, setAssignee] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (visible) {
      setTitle('');
      setCategory(0);
      setAssignee(null);
      setError(null);
    }
  }, [visible]);

  async function submit() {
    if (!token) return;
    if (!title.trim()) {
      setError('Введите название задачи');
      return;
    }
    setBusy(true);
    try {
      await api.createChore(token, {
        apartmentId: context.apartmentId,
        title: title.trim(),
        description: null,
        category,
        assignedUserId: assignee,
        dueDate: null,
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
          <Text style={[TYPE.headline, { color: T.text }]}>Новая задача</Text>
          <Pressable onPress={submit} disabled={busy}>
            <Text
              style={[
                TYPE.bodyMed,
                { color: busy ? T.text3 : T.accent, fontWeight: '700' },
              ]}
            >
              {busy ? '...' : 'Создать'}
            </Text>
          </Pressable>
        </View>
        <ScrollView contentContainerStyle={{ padding: 16, gap: 16 }}>
          <View>
            <Text style={[TYPE.footMed, { color: T.text2, marginBottom: 6, paddingLeft: 4 }]}>
              Название
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
                value={title}
                onChangeText={setTitle}
                placeholder="Например: помыть посуду"
                placeholderTextColor={T.text3}
                style={{ fontSize: 15, color: T.text, padding: 0 }}
              />
            </View>
          </View>

          <View>
            <Text style={[TYPE.footMed, { color: T.text2, marginBottom: 6, paddingLeft: 4 }]}>
              Категория
            </Text>
            <ScrollView
              horizontal
              showsHorizontalScrollIndicator={false}
              contentContainerStyle={{ gap: 8, paddingHorizontal: 4 }}
            >
              {CATEGORY_OPTIONS.map((o) => {
                const active = o.v === category;
                return (
                  <Pressable
                    key={o.v}
                    onPress={() => setCategory(o.v)}
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
          </View>

          <View>
            <Text style={[TYPE.footMed, { color: T.text2, marginBottom: 8, paddingLeft: 4 }]}>
              Кому (опционально)
            </Text>
            <View style={{ gap: 8 }}>
              <Pressable
                onPress={() => setAssignee(null)}
                style={{
                  flexDirection: 'row',
                  alignItems: 'center',
                  gap: 12,
                  padding: 12,
                  backgroundColor: assignee === null ? T.accentSoft : '#fff',
                  borderColor: assignee === null ? T.accent : T.border,
                  borderWidth: 1,
                  borderRadius: RADIUS.input,
                }}
              >
                <View
                  style={{
                    width: 32,
                    height: 32,
                    borderRadius: 16,
                    backgroundColor: 'rgba(20,24,40,0.06)',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <Icon name="users" size={18} color={T.text2} />
                </View>
                <Text style={[TYPE.body, { color: T.text, flex: 1 }]}>Без исполнителя</Text>
              </Pressable>
              {context.roommates.map((r) => (
                <Pressable
                  key={r.userId}
                  onPress={() => setAssignee(r.userId)}
                  style={{
                    flexDirection: 'row',
                    alignItems: 'center',
                    gap: 12,
                    padding: 12,
                    backgroundColor: assignee === r.userId ? T.accentSoft : '#fff',
                    borderColor: assignee === r.userId ? T.accent : T.border,
                    borderWidth: 1,
                    borderRadius: RADIUS.input,
                  }}
                >
                  <Avatar initial={r.name.charAt(0)} size={32} color={T.violet} />
                  <Text style={[TYPE.body, { color: T.text, flex: 1 }]}>
                    {r.name}
                    {r.isMe ? ' (вы)' : ''}
                  </Text>
                  {assignee === r.userId && <Icon name="check" size={18} color={T.accent} />}
                </Pressable>
              ))}
            </View>
          </View>

          {error && <Pill color={T.danger} soft={T.dangerSoft}>{error}</Pill>}
        </ScrollView>
      </View>
    </Modal>
  );
}
