import { Fragment, useCallback, useEffect, useState } from 'react';
import {
  Linking,
  Pressable,
  RefreshControl,
  ScrollView,
  Text,
  View,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { api } from '../api/client';
import { MyApartmentContextDto, ResidentNotificationDto } from '../api/types';
import {
  Divider,
  Glass,
  GlassIconBtn,
  GradientBackground,
  Icon,
  IconName,
  PageHeader,
  Pill,
  SectionLabel,
} from '../components';
import { KEYS_APP_URL } from '../config';
import { useSession } from '../state/SessionContext';
import { GRADIENTS, SHADOW, T, TYPE } from '../theme/tokens';

export function HomeScreen() {
  const { token } = useSession();
  const [context, setContext] = useState<MyApartmentContextDto | null>(null);
  const [notifications, setNotifications] = useState<ResidentNotificationDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    try {
      const [ctx, notifs] = await Promise.all([
        api.getMyContext(token),
        api.getNotifications(token),
      ]);
      setContext(ctx);
      setNotifications(notifs);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось загрузить');
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  async function onMarkRead(id: string) {
    if (!token) return;
    try {
      await api.markNotificationRead(token, id);
      setNotifications((current) =>
        current.map((n) => (n.id === id ? { ...n, isRead: true } : n))
      );
    } catch {
      /* swallow — pull-to-refresh покажет реальное состояние */
    }
  }

  const unreadCount = notifications.filter((n) => !n.isRead).length;
  const recent = notifications.slice(0, 4);

  return (
    <GradientBackground>
      <PageHeader
        title="Главная"
        trailing={<GlassIconBtn icon="bell" badge={unreadCount > 0} />}
      />
      <ScrollView
        contentContainerStyle={{
          paddingHorizontal: 16,
          paddingTop: 16,
          paddingBottom: 120,
        }}
        refreshControl={<RefreshControl refreshing={loading} onRefresh={refresh} />}
      >
        <DoorCard context={context} />

        <View style={{ marginTop: 18 }}>
          <SectionLabel
            trailing={
              unreadCount > 0 ? (
                <Text style={[TYPE.footMed, { color: T.accent }]}>{unreadCount} новых</Text>
              ) : undefined
            }
          >
            Уведомления
          </SectionLabel>
          {recent.length === 0 ? (
            <Glass padding={20} radius={20}>
              <Text style={[TYPE.body, { color: T.text2, textAlign: 'center' }]}>
                {loading ? 'Загружаем…' : 'Пока тихо.'}
              </Text>
            </Glass>
          ) : (
            <Glass padding={0} radius={20}>
              {recent.map((n, i) => (
                <Fragment key={n.id}>
                  <NotifRow notice={n} onMarkRead={() => void onMarkRead(n.id)} />
                  {i < recent.length - 1 && <Divider inset={68} />}
                </Fragment>
              ))}
            </Glass>
          )}
        </View>

        <View style={{ marginTop: 18 }}>
          <SectionLabel>Моя квартира</SectionLabel>
          <ApartmentSummary context={context} />
        </View>

        {error && (
          <View style={{ marginTop: 16 }}>
            <Glass padding={14} radius={16}>
              <Text style={[TYPE.footnote, { color: T.danger }]}>{error}</Text>
            </Glass>
          </View>
        )}
      </ScrollView>
    </GradientBackground>
  );
}

function DoorCard({ context }: { context: MyApartmentContextDto | null }) {
  const room = context?.rooms[0];
  const subtitle = context
    ? room
      ? `Комната ${room.number} · ${context.buildingName ?? 'Общие зоны'}`
      : `${context.unitNumber ?? context.apartmentName} · ${context.buildingName ?? 'Общие зоны'}`
    : 'Комната ещё не назначена';

  return (
    <Pressable
      onPress={() => void Linking.openURL(KEYS_APP_URL)}
      style={({ pressed }) => [
        {
          borderRadius: 26,
          overflow: 'hidden',
          opacity: pressed ? 0.92 : 1,
        },
        SHADOW.doorCard,
      ]}
    >
      <LinearGradient
        colors={[...GRADIENTS.door]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={{ padding: 22, alignItems: 'center' }}
      >
        <View
          style={{
            width: 64,
            height: 64,
            borderRadius: 18,
            backgroundColor: 'rgba(255,255,255,0.22)',
            borderColor: 'rgba(255,255,255,0.35)',
            borderWidth: 1,
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          <Icon name="door" size={32} color="#fff" />
        </View>
        <Text style={[TYPE.title, { color: '#fff', marginTop: 12 }]}>Разблокировать</Text>
        <Text style={[TYPE.footnote, { color: 'rgba(255,255,255,0.85)', marginTop: 4 }]}>
          {subtitle}
        </Text>
        <View
          style={{
            flexDirection: 'row',
            alignItems: 'center',
            gap: 6,
            marginTop: 12,
            opacity: 0.9,
          }}
        >
          <Dot color="#fff" />
          <Dot color="rgba(255,255,255,0.7)" />
          <Dot color="rgba(255,255,255,0.45)" />
          <Text style={[TYPE.caption, { color: '#fff', marginLeft: 4 }]}>
            Откроется в приложении Ключи
          </Text>
        </View>
      </LinearGradient>
    </Pressable>
  );
}

function Dot({ color }: { color: string }) {
  return (
    <View style={{ width: 6, height: 6, borderRadius: 3, backgroundColor: color }} />
  );
}

type NotifTone = 'danger' | 'warning' | 'info';
const TONE: Record<NotifTone, { bg: string; fg: string; icon: IconName }> = {
  danger: { bg: T.dangerSoft, fg: T.danger, icon: 'alert' },
  warning: { bg: T.warningSoft, fg: T.warning, icon: 'bell' },
  info: { bg: T.infoSoft, fg: T.info, icon: 'info' },
};

function pickTone(notice: ResidentNotificationDto): NotifTone {
  if (notice.isImportant && !notice.isRead) return 'danger';
  if (notice.isImportant) return 'warning';
  return 'info';
}

function NotifRow({
  notice,
  onMarkRead,
}: {
  notice: ResidentNotificationDto;
  onMarkRead: () => void;
}) {
  const tone = TONE[pickTone(notice)];
  return (
    <Pressable onPress={notice.isRead ? undefined : onMarkRead}>
      <View
        style={{
          flexDirection: 'row',
          alignItems: 'center',
          gap: 12,
          paddingHorizontal: 14,
          paddingVertical: 14,
        }}
      >
        <View
          style={{
            width: 38,
            height: 38,
            borderRadius: 12,
            backgroundColor: tone.bg,
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          <Icon name={tone.icon} size={20} color={tone.fg} />
        </View>
        <View style={{ flex: 1 }}>
          <Text
            style={[
              TYPE.bodyMed,
              { color: T.text, fontWeight: notice.isRead ? '500' : '700' },
            ]}
          >
            {notice.title}
          </Text>
          <Text
            style={[TYPE.footnote, { color: T.text2, marginTop: 1 }]}
            numberOfLines={2}
          >
            {notice.body}
          </Text>
        </View>
        {!notice.isRead && (
          <View
            style={{
              width: 8,
              height: 8,
              borderRadius: 4,
              backgroundColor: T.accent,
            }}
          />
        )}
      </View>
    </Pressable>
  );
}

function ApartmentSummary({ context }: { context: MyApartmentContextDto | null }) {
  if (!context) {
    return (
      <Glass padding={20} radius={20}>
        <Text style={[TYPE.body, { color: T.text2, textAlign: 'center' }]}>
          Квартира ещё не назначена. Свяжитесь с администратором.
        </Text>
      </Glass>
    );
  }

  const room = context.rooms[0];
  return (
    <Glass padding={16} radius={20}>
      <Text style={[TYPE.footnote, { color: T.text2 }]}>
        {context.buildingName ?? 'Co-Living'}
      </Text>
      <Text style={[TYPE.title, { color: T.text, marginTop: 4 }]}>
        {context.unitNumber ? `Юнит ${context.unitNumber}` : context.apartmentName}
      </Text>
      <View style={{ flexDirection: 'row', gap: 8, marginTop: 10, flexWrap: 'wrap' }}>
        {room && (
          <Pill color={T.accent} soft={T.accentSoft}>
            {`Комната ${room.number}`}
          </Pill>
        )}
        {room?.typeLabel && (
          <Pill color={T.violet} soft={T.violetSoft}>
            {room.typeLabel}
          </Pill>
        )}
      </View>
    </Glass>
  );
}
