import { useCallback, useEffect, useMemo, useState } from 'react';
import { Alert, Linking, RefreshControl, ScrollView, Text, View } from 'react-native';
import { api } from '../api/client';
import { MyApartmentContextDto, RoommateDto } from '../api/types';
import {
  Avatar,
  Button,
  Divider,
  Glass,
  GradientBackground,
  Icon,
  ListRow,
  PageHeader,
  Pill,
  SectionLabel,
} from '../components';
import { PRIVACY_POLICY_URL, SUPPORT_EMAIL } from '../config';
import { useSession } from '../state/SessionContext';
import { T, TYPE } from '../theme/tokens';

const ROOMMATE_PALETTE = [T.accent, T.violet, T.success, T.warning, T.danger, T.info];

export function ProfileScreen() {
  const { token, logout } = useSession();
  const [context, setContext] = useState<MyApartmentContextDto | null>(null);
  const [loading, setLoading] = useState(false);

  const refresh = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    try {
      const ctx = await api.getMyContext(token);
      setContext(ctx);
    } catch {
      /* swallow — Профиль работает и без квартиры */
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  function openSupport(subject: string) {
    void Linking.openURL(`mailto:${SUPPORT_EMAIL}?subject=${encodeURIComponent(subject)}`);
  }

  function confirmLogout() {
    Alert.alert('Выйти из аккаунта?', 'Вам нужно будет войти заново.', [
      { text: 'Отмена', style: 'cancel' },
      { text: 'Выйти', style: 'destructive', onPress: () => void logout() },
    ]);
  }

  const myName = context?.meName?.trim() || 'Жилец';
  const initial = myName.charAt(0).toUpperCase();
  const roommates = context?.roommates ?? [];
  const others = useMemo(() => roommates.filter((r) => !r.isMe), [roommates]);
  const isSolo = others.length === 0;
  const myRoom = roommates.find((r) => r.isMe)?.roomNumber ?? null;

  return (
    <GradientBackground>
      <PageHeader title="Профиль" />
      <ScrollView
        contentContainerStyle={{
          paddingHorizontal: 16,
          paddingTop: 16,
          paddingBottom: 140,
        }}
        refreshControl={<RefreshControl refreshing={loading} onRefresh={refresh} />}
      >
        <Glass padding={16} radius={22}>
          <View style={{ flexDirection: 'row', alignItems: 'center', gap: 14 }}>
            <Avatar initial={initial} size={56} color={T.accent} />
            <View style={{ flex: 1 }}>
              <Text style={[TYPE.headline, { color: T.text }]} numberOfLines={1}>
                {myName}
              </Text>
              <Text
                style={[TYPE.footnote, { color: T.text2, marginTop: 2 }]}
                numberOfLines={1}
              >
                {context?.buildingName ?? 'Co-Living OS'}
                {myRoom ? ` · комната ${myRoom}` : ''}
              </Text>
              <View
                style={{ flexDirection: 'row', gap: 6, marginTop: 8, flexWrap: 'wrap' }}
              >
                <Pill color={T.accent} soft={T.accentSoft}>Жилец</Pill>
                {isSolo ? (
                  <Pill color={T.text2} soft="rgba(20,24,40,0.06)">Solo mode</Pill>
                ) : (
                  <Pill color={T.violet} soft={T.violetSoft}>Shared mode</Pill>
                )}
              </View>
            </View>
          </View>
        </Glass>

        <View style={{ marginTop: 18 }}>
          <SectionLabel>{isSolo ? 'Квартира' : 'Жильцы'}</SectionLabel>
          {isSolo ? (
            <Glass padding={22} radius={26} style={{ alignItems: 'center' }}>
              <View
                style={{
                  width: 56,
                  height: 56,
                  borderRadius: 18,
                  backgroundColor: T.accentSoft,
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <Icon name="users" size={28} color={T.accent} />
              </View>
              <Text style={[TYPE.title, { color: T.text, marginTop: 12 }]}>
                Живёте одни
              </Text>
              <Text
                style={[
                  TYPE.body,
                  {
                    color: T.text2,
                    marginTop: 6,
                    textAlign: 'center',
                    maxWidth: 280,
                  },
                ]}
              >
                Когда появятся соседи — откроется вкладка «Соседи» и общий чат.
              </Text>
            </Glass>
          ) : (
            <RoommatesList roommates={roommates} />
          )}
        </View>

        <View style={{ marginTop: 18 }}>
          <SectionLabel>Аккаунт</SectionLabel>
          <Glass padding={0} radius={20}>
            <ListRow
              icon={<Icon name="lock" size={18} color={T.text2} />}
              title="Сменить пароль"
              trailing={<Pill color={T.text2} soft="rgba(20,24,40,0.06)">Скоро</Pill>}
            />
            <ListRow
              icon={<Icon name="bell" size={18} color={T.text2} />}
              title="Уведомления"
              trailing="Все"
              chevron
            />
            <ListRow
              icon={<Icon name="doc" size={18} color={T.text2} />}
              title="Документы"
              trailing={<Pill color={T.text2} soft="rgba(20,24,40,0.06)">Скоро</Pill>}
              isLast
            />
          </Glass>
        </View>

        <View style={{ marginTop: 18 }}>
          <SectionLabel>Поддержка</SectionLabel>
          <Glass padding={0} radius={20}>
            <ListRow
              icon={<Icon name="mail" size={18} color={T.text2} />}
              title="Связаться с поддержкой"
              chevron
              onPress={() => openSupport('Помощь жильцу')}
            />
            <ListRow
              icon={<Icon name="doc" size={18} color={T.text2} />}
              title="Политика конфиденциальности"
              chevron
              onPress={() => void Linking.openURL(PRIVACY_POLICY_URL)}
            />
            <ListRow
              icon={<Icon name="ban" size={18} color={T.danger} />}
              iconBg={T.dangerSoft}
              title="Удалить аккаунт"
              danger
              onPress={() => openSupport('Запрос на удаление аккаунта')}
              isLast
            />
          </Glass>
        </View>

        <View style={{ marginTop: 24 }}>
          <Button
            kind="glass"
            full
            leading={<Icon name="log-out" size={18} color={T.danger} />}
            textColor={T.danger}
            onPress={confirmLogout}
          >
            Выйти из аккаунта
          </Button>
        </View>

        <Text
          style={[
            TYPE.caption,
            { color: T.text3, textAlign: 'center', marginTop: 16 },
          ]}
        >
          Co-Living OS · v1.0.0
        </Text>
      </ScrollView>
    </GradientBackground>
  );
}

function RoommatesList({ roommates }: { roommates: RoommateDto[] }) {
  // Сортируем: я сверху, потом остальные по дате заселения.
  const sorted = [...roommates].sort((a, b) => {
    if (a.isMe !== b.isMe) return a.isMe ? -1 : 1;
    return new Date(a.joinedAt).getTime() - new Date(b.joinedAt).getTime();
  });
  return (
    <Glass padding={0} radius={24}>
      <View style={{ paddingHorizontal: 16, paddingTop: 14 }}>
        <Text style={[TYPE.caption, { color: T.text2 }]}>
          В КВАРТИРЕ · {sorted.length}
        </Text>
      </View>
      <View style={{ padding: 16, gap: 12 }}>
        {sorted.map((r, idx) => (
          <RoommateRow key={r.userId} roommate={r} colorIdx={idx} />
        ))}
      </View>
    </Glass>
  );
}

function RoommateRow({ roommate, colorIdx }: { roommate: RoommateDto; colorIdx: number }) {
  const color = ROOMMATE_PALETTE[colorIdx % ROOMMATE_PALETTE.length];
  const displayName = (roommate.name?.trim() || 'Жилец') + (roommate.isMe ? ' (вы)' : '');
  const joined = new Date(roommate.joinedAt);
  const tag = roommate.roomNumber
    ? `комната ${roommate.roomNumber} · с ${joined.toLocaleDateString('ru-RU', { month: 'short', year: '2-digit' })}`
    : `с ${joined.toLocaleDateString('ru-RU', { month: 'short', year: '2-digit' })}`;
  return (
    <View style={{ flexDirection: 'row', alignItems: 'center', gap: 10 }}>
      <Avatar initial={displayName.charAt(0)} size={32} color={color} />
      <View style={{ flex: 1 }}>
        <Text style={[TYPE.body, { color: T.text, fontWeight: '500' }]} numberOfLines={1}>
          {displayName}
        </Text>
        <Text style={[TYPE.caption, { color: T.text2, marginTop: 1 }]} numberOfLines={1}>
          {tag}
        </Text>
      </View>
    </View>
  );
}
