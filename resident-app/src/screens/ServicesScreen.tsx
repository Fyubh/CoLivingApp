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
import { api } from '../api/client';
import {
  CreateMaintenanceRequest,
  MaintenanceCategory,
  MaintenancePriority,
  MaintenanceRequestDto,
  MaintenanceStatus,
  MyApartmentContextDto,
} from '../api/types';
import {
  Button,
  Divider,
  Glass,
  GradientBackground,
  Icon,
  IconName,
  PageHeader,
  Pill,
  SectionLabel,
} from '../components';
import { useSession } from '../state/SessionContext';
import { RADIUS, T, TYPE } from '../theme/tokens';

const CATEGORY_LABEL: Record<MaintenanceCategory, string> = {
  Plumbing: 'Сантехника',
  Electric: 'Электрика',
  Furniture: 'Мебель',
  Appliance: 'Техника',
  Hvac: 'Климат',
  WindowsAndDoors: 'Окна/двери',
  Cleaning: 'Уборка',
  Internet: 'Интернет',
  Other: 'Другое',
};
const CATEGORIES: MaintenanceCategory[] = [
  'Plumbing',
  'Electric',
  'Furniture',
  'Appliance',
  'Hvac',
  'WindowsAndDoors',
  'Cleaning',
  'Internet',
  'Other',
];

const PRIORITY_LABEL: Record<MaintenancePriority, string> = {
  Low: 'Низкий',
  Normal: 'Обычный',
  High: 'Высокий',
  Urgent: 'Срочно',
};
const PRIORITIES: MaintenancePriority[] = ['Low', 'Normal', 'High', 'Urgent'];

const STATUS_LABEL: Record<MaintenanceStatus, string> = {
  Reported: 'Отправлено',
  Acknowledged: 'Принято',
  Assigned: 'Назначено',
  InProgress: 'В работе',
  Completed: 'Готово',
  Cancelled: 'Отменено',
  Rejected: 'Отклонено',
};
const STATUS_TONE: Record<MaintenanceStatus, [string, string]> = {
  Reported: [T.text2, 'rgba(20,24,40,0.06)'],
  Acknowledged: [T.info, T.infoSoft],
  Assigned: [T.violet, T.violetSoft],
  InProgress: [T.warning, T.warningSoft],
  Completed: [T.success, T.successSoft],
  Cancelled: [T.text3, 'rgba(20,24,40,0.06)'],
  Rejected: [T.danger, T.dangerSoft],
};

type LocationOption = {
  key: string;
  label: string;
  payload: Pick<CreateMaintenanceRequest, 'roomId' | 'apartmentId' | 'buildingId'>;
};

function buildLocationOptions(context: MyApartmentContextDto | null): LocationOption[] {
  if (!context) return [];
  const out: LocationOption[] = [];
  context.rooms.forEach((r) => {
    out.push({
      key: `room:${r.id}`,
      label: `Комната ${r.number}`,
      payload: { roomId: r.id },
    });
  });
  out.push({
    key: `apt:${context.apartmentId}`,
    label: context.unitNumber ? `Юнит ${context.unitNumber}` : 'Моя квартира',
    payload: { apartmentId: context.apartmentId },
  });
  if (context.buildingId) {
    out.push({
      key: `b:${context.buildingId}`,
      label: 'Общие зоны здания',
      payload: { buildingId: context.buildingId },
    });
  }
  return out;
}

export function ServicesScreen() {
  const { token } = useSession();
  const [context, setContext] = useState<MyApartmentContextDto | null>(null);
  const [items, setItems] = useState<MaintenanceRequestDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const refresh = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    try {
      const [ctx, list] = await Promise.all([
        api.getMyContext(token),
        api.getMaintenance(token),
      ]);
      setContext(ctx);
      setItems(list);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось загрузить');
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  async function cancelItem(id: string) {
    if (!token) return;
    try {
      await api.cancelMaintenance(token, id);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось отменить');
    }
  }

  async function rateItem(id: string, rating: number) {
    if (!token) return;
    try {
      await api.rateMaintenance(token, id, rating);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось оценить');
    }
  }

  async function submitForm(payload: CreateMaintenanceRequest) {
    if (!token) return;
    setSubmitting(true);
    try {
      await api.createMaintenance(token, payload);
      setShowForm(false);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось отправить');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <GradientBackground>
      <PageHeader title="Сервисы" />
      <ScrollView
        contentContainerStyle={{
          paddingHorizontal: 16,
          paddingTop: 16,
          paddingBottom: 120,
        }}
        refreshControl={<RefreshControl refreshing={loading} onRefresh={refresh} />}
      >
        <ManagementSection />
        <BookingsSection />
        <VipServicesSection />
        <MaintenanceSection
          items={items}
          onCreate={() => setShowForm(true)}
          onCancel={cancelItem}
          onRate={rateItem}
        />

        {error && (
          <View style={{ marginTop: 16 }}>
            <Glass padding={14} radius={16}>
              <Text style={[TYPE.footnote, { color: T.danger }]}>{error}</Text>
            </Glass>
          </View>
        )}
      </ScrollView>

      <MaintenanceFormModal
        visible={showForm}
        onClose={() => setShowForm(false)}
        onSubmit={submitForm}
        context={context}
        submitting={submitting}
      />
    </GradientBackground>
  );
}

// ─── Sections ───────────────────────────────────────────────────────────

function ManagementSection() {
  return (
    <View style={{ marginBottom: 18 }}>
      <SectionLabel>Управление жильём</SectionLabel>
      <Glass padding={0} radius={20}>
        <ManageRow title="Контракт аренды" sub="истекает позже" />
        <Divider inset={16} />
        <ManageRow title="Справка о проживании" sub="Potvrzení o ubytování" isLast />
      </Glass>
    </View>
  );
}

function ManageRow({ title, sub, isLast }: { title: string; sub: string; isLast?: boolean }) {
  return (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        paddingHorizontal: 16,
        paddingVertical: 14,
        gap: 10,
      }}
    >
      <View style={{ flex: 1 }}>
        <Text style={[TYPE.bodyMed, { color: T.text }]}>{title}</Text>
        <Text style={[TYPE.footnote, { color: T.text2, marginTop: 2 }]}>{sub}</Text>
      </View>
      <Pill color={T.text2} soft="rgba(20,24,40,0.06)">Скоро</Pill>
      {!isLast && null}
    </View>
  );
}

const BOOKINGS: { icon: IconName; name: string; sub: string }[] = [
  { icon: 'droplet', name: 'Прачечная', sub: 'свободно сейчас' },
  { icon: 'laptop', name: 'Коворкинг', sub: '2 / 8 заняты' },
  { icon: 'film', name: 'Кинокомната', sub: 'бронь до 23:00' },
  { icon: 'dumbbell', name: 'Спортзал', sub: 'свободно' },
];

function BookingsSection() {
  return (
    <View style={{ marginBottom: 18 }}>
      <SectionLabel>Бронирование зон</SectionLabel>
      <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: 12 }}>
        {BOOKINGS.map((b) => (
          <View key={b.name} style={{ flexBasis: '48%', flexGrow: 1 }}>
            <BookCard icon={b.icon} name={b.name} sub={b.sub} />
          </View>
        ))}
      </View>
    </View>
  );
}

function BookCard({ icon, name, sub }: { icon: IconName; name: string; sub: string }) {
  return (
    <Glass padding={16} radius={20} style={{ alignItems: 'center', position: 'relative' }}>
      <View
        style={{
          width: 48,
          height: 48,
          borderRadius: 14,
          backgroundColor: T.accentSoft,
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Icon name={icon} size={24} color={T.accent} />
      </View>
      <Text style={[TYPE.bodyMed, { color: T.text, marginTop: 8 }]}>{name}</Text>
      <Text style={[TYPE.caption, { color: T.text2, marginTop: 2 }]}>{sub}</Text>
      <View style={{ position: 'absolute', top: 10, right: 10 }}>
        <Pill color={T.text2} soft="rgba(20,24,40,0.08)">Скоро</Pill>
      </View>
    </Glass>
  );
}

function VipServicesSection() {
  return (
    <View style={{ marginBottom: 18 }}>
      <SectionLabel>VIP-услуги & аренда</SectionLabel>
      <Glass padding={0} radius={20}>
        <VipRow
          icon="sparkle"
          iconBg="rgba(232,154,42,0.15)"
          iconColor={T.warning}
          title="Уборка комнаты"
          sub="$15 / раз"
        />
        <Divider inset={68} />
        <VipRow
          icon="gamepad"
          iconBg="rgba(125,107,255,0.15)"
          iconColor={T.violet}
          title="Аренда PS5"
          sub="с локера ресепшн"
          isLast
        />
      </Glass>
    </View>
  );
}

function VipRow({
  icon,
  iconBg,
  iconColor,
  title,
  sub,
  isLast,
}: {
  icon: IconName;
  iconBg: string;
  iconColor: string;
  title: string;
  sub: string;
  isLast?: boolean;
}) {
  return (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        paddingHorizontal: 14,
        paddingVertical: 14,
        gap: 12,
      }}
    >
      <View
        style={{
          width: 40,
          height: 40,
          borderRadius: 12,
          backgroundColor: iconBg,
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Icon name={icon} size={22} color={iconColor} />
      </View>
      <View style={{ flex: 1 }}>
        <Text style={[TYPE.bodyMed, { color: T.text }]}>{title}</Text>
        <Text style={[TYPE.footnote, { color: T.text2, marginTop: 1 }]}>{sub}</Text>
      </View>
      <Pill color={T.text2} soft="rgba(20,24,40,0.06)">Скоро</Pill>
      {!isLast && null}
    </View>
  );
}

// ─── Maintenance ────────────────────────────────────────────────────────

function MaintenanceSection({
  items,
  onCreate,
  onCancel,
  onRate,
}: {
  items: MaintenanceRequestDto[];
  onCreate: () => void;
  onCancel: (id: string) => Promise<void>;
  onRate: (id: string, rating: number) => Promise<void>;
}) {
  return (
    <View>
      <SectionLabel
        trailing={
          <Pressable onPress={onCreate}>
            <Text style={[TYPE.footMed, { color: T.accent }]}>+ Новая</Text>
          </Pressable>
        }
      >
        Заявки на ремонт
      </SectionLabel>
      {items.length === 0 ? (
        <Glass padding={20} radius={20}>
          <Text style={[TYPE.body, { color: T.text2, textAlign: 'center' }]}>
            Заявок пока нет.
          </Text>
          <View style={{ alignItems: 'center', marginTop: 12 }}>
            <Button
              kind="secondary"
              size="sm"
              leading={<Icon name="plus" size={14} color={T.accent} />}
              onPress={onCreate}
            >
              Создать заявку
            </Button>
          </View>
        </Glass>
      ) : (
        <Glass padding={0} radius={20}>
          {items.map((item, idx) => (
            <Fragment key={item.id}>
              <MaintenanceRow item={item} onCancel={onCancel} onRate={onRate} />
              {idx < items.length - 1 && <Divider inset={16} />}
            </Fragment>
          ))}
        </Glass>
      )}
    </View>
  );
}

function MaintenanceRow({
  item,
  onCancel,
  onRate,
}: {
  item: MaintenanceRequestDto;
  onCancel: (id: string) => Promise<void>;
  onRate: (id: string, rating: number) => Promise<void>;
}) {
  const [color, bg] = STATUS_TONE[item.status];
  const canCancel = ['Reported', 'Acknowledged', 'Assigned'].includes(item.status);
  const canRate = item.status === 'Completed' && !item.residentRating;

  return (
    <View style={{ paddingHorizontal: 14, paddingVertical: 14 }}>
      <View style={{ flexDirection: 'row', alignItems: 'flex-start', gap: 10 }}>
        <View style={{ flex: 1 }}>
          <Text style={[TYPE.bodyMed, { color: T.text }]}>{item.title}</Text>
          <Text
            style={[TYPE.footnote, { color: T.text2, marginTop: 2 }]}
            numberOfLines={3}
          >
            {item.description}
          </Text>
          {item.assignedStaffName && (
            <Text style={[TYPE.caption, { color: T.text3, marginTop: 4 }]}>
              Назначено: {item.assignedStaffName}
            </Text>
          )}
          {item.completionNotes && (
            <Text style={[TYPE.footnote, { color: T.text, marginTop: 4 }]}>
              {item.completionNotes}
            </Text>
          )}
        </View>
        <Pill color={color} soft={bg}>
          {STATUS_LABEL[item.status]}
        </Pill>
      </View>

      {canCancel && (
        <View style={{ marginTop: 10, flexDirection: 'row' }}>
          <Button
            kind="glass"
            size="sm"
            onPress={() => void onCancel(item.id)}
          >
            Отменить
          </Button>
        </View>
      )}

      {canRate && (
        <View style={{ marginTop: 10, flexDirection: 'row', alignItems: 'center', gap: 6 }}>
          <Text style={[TYPE.footMed, { color: T.text2, marginRight: 4 }]}>Оцените:</Text>
          {[1, 2, 3, 4, 5].map((r) => (
            <Pressable
              key={r}
              onPress={() => void onRate(item.id, r)}
              style={{
                width: 36,
                height: 36,
                borderRadius: 10,
                backgroundColor: T.accentSoft,
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Text style={[TYPE.bodyMed, { color: T.accent, fontWeight: '700' }]}>{r}</Text>
            </Pressable>
          ))}
        </View>
      )}

      {item.residentRating && (
        <Text style={[TYPE.caption, { color: T.text3, marginTop: 6 }]}>
          Ваша оценка: {item.residentRating}/5
        </Text>
      )}
    </View>
  );
}

// ─── Form modal ─────────────────────────────────────────────────────────

function MaintenanceFormModal({
  visible,
  onClose,
  onSubmit,
  context,
  submitting,
}: {
  visible: boolean;
  onClose: () => void;
  onSubmit: (payload: CreateMaintenanceRequest) => Promise<void>;
  context: MyApartmentContextDto | null;
  submitting: boolean;
}) {
  const insets = useSafeAreaInsets();
  const locations = useMemo(() => buildLocationOptions(context), [context]);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState<MaintenanceCategory>('Plumbing');
  const [priority, setPriority] = useState<MaintenancePriority>('Normal');
  const [locKey, setLocKey] = useState<string>('');

  useEffect(() => {
    if (visible) {
      setTitle('');
      setDescription('');
      setCategory('Plumbing');
      setPriority('Normal');
      setLocKey(locations[0]?.key ?? '');
    }
  }, [visible, locations]);

  const selectedLoc = locations.find((l) => l.key === locKey) ?? locations[0];
  const canSubmit =
    !submitting && !!selectedLoc && title.trim().length > 0 && description.trim().length > 0;

  function handleSubmit() {
    if (!selectedLoc) return;
    void onSubmit({
      title: title.trim(),
      description: description.trim(),
      category,
      priority,
      ...selectedLoc.payload,
    });
  }

  return (
    <Modal visible={visible} animationType="slide" presentationStyle="pageSheet" onRequestClose={onClose}>
      <GradientBackground>
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
            backgroundColor: 'rgba(255,255,255,0.55)',
          }}
        >
          <Pressable onPress={onClose}>
            <Text style={[TYPE.bodyMed, { color: T.accent }]}>Закрыть</Text>
          </Pressable>
          <Text style={[TYPE.headline, { color: T.text }]}>Новая заявка</Text>
          <Pressable onPress={handleSubmit} disabled={!canSubmit}>
            <Text
              style={[
                TYPE.bodyMed,
                { color: canSubmit ? T.accent : T.text3, fontWeight: '700' },
              ]}
            >
              {submitting ? '...' : 'Отправить'}
            </Text>
          </Pressable>
        </View>

        <ScrollView
          contentContainerStyle={{ padding: 16, paddingBottom: insets.bottom + 24, gap: 16 }}
          keyboardShouldPersistTaps="handled"
        >
          <Field label="Категория">
            <ChipPicker
              value={category}
              options={CATEGORIES}
              labels={CATEGORY_LABEL}
              onChange={setCategory}
            />
          </Field>
          <Field label="Срочность">
            <ChipPicker
              value={priority}
              options={PRIORITIES}
              labels={PRIORITY_LABEL}
              onChange={setPriority}
            />
          </Field>
          {locations.length > 0 && (
            <Field label="Где?">
              <ChipPicker
                value={selectedLoc?.key ?? ''}
                options={locations.map((l) => l.key)}
                labels={Object.fromEntries(locations.map((l) => [l.key, l.label]))}
                onChange={setLocKey}
              />
            </Field>
          )}
          <Field label="Заголовок">
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
                placeholder="Например: течёт кран"
                placeholderTextColor={T.text3}
                style={{ fontSize: 15, color: T.text, padding: 0 }}
              />
            </View>
          </Field>
          <Field label="Описание">
            <View
              style={{
                backgroundColor: '#fff',
                borderColor: T.border,
                borderWidth: 1,
                borderRadius: RADIUS.input,
                paddingHorizontal: 14,
                paddingVertical: 12,
                minHeight: 120,
              }}
            >
              <TextInput
                value={description}
                onChangeText={setDescription}
                placeholder="Подробнее опишите проблему"
                placeholderTextColor={T.text3}
                multiline
                textAlignVertical="top"
                style={{ fontSize: 15, color: T.text, padding: 0, minHeight: 96 }}
              />
            </View>
          </Field>
          {!context && (
            <Glass padding={14} radius={16}>
              <Text style={[TYPE.footnote, { color: T.warning }]}>
                Квартира не назначена. Создание заявки невозможно.
              </Text>
            </Glass>
          )}
        </ScrollView>
      </GradientBackground>
    </Modal>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <View>
      <Text style={[TYPE.footMed, { color: T.text2, marginBottom: 6, paddingLeft: 4 }]}>
        {label}
      </Text>
      {children}
    </View>
  );
}

function ChipPicker<O extends string>({
  value,
  options,
  labels,
  onChange,
}: {
  value: O;
  options: readonly O[];
  labels?: Record<string, string>;
  onChange: (v: O) => void;
}) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={{ gap: 8, paddingHorizontal: 4 }}
    >
      {options.map((o) => {
        const active = o === value;
        return (
          <Pressable
            key={o}
            onPress={() => onChange(o)}
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
              {labels?.[o] ?? o}
            </Text>
          </Pressable>
        );
      })}
    </ScrollView>
  );
}
