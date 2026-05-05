import { useCallback, useEffect, useRef, useState } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from 'react-native';
import { api } from '../../api/client';
import { ChatMessageDto, MyApartmentContextDto } from '../../api/types';
import { Avatar, Glass, Icon } from '../../components';
import { useSession } from '../../state/SessionContext';
import { RADIUS, T, TYPE } from '../../theme/tokens';

const ROOMMATE_PALETTE = [T.violet, T.success, T.warning, T.danger, T.info];

export function ChatTab({ context }: { context: MyApartmentContextDto }) {
  const { token } = useSession();
  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [text, setText] = useState('');
  const [sending, setSending] = useState(false);
  const scrollRef = useRef<ScrollView>(null);

  const refresh = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    try {
      const list = await api.getChatHistory(token, context.apartmentId);
      // backend возвращает по убыванию даты; разворачиваем для отображения снизу
      setMessages([...list].reverse());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setLoading(false);
    }
  }, [token, context.apartmentId]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  // Простой polling каждые 5с — вместо SignalR на этом этапе.
  useEffect(() => {
    const id = setInterval(() => void refresh(), 5000);
    return () => clearInterval(id);
  }, [refresh]);

  async function send() {
    if (!token || !text.trim() || sending) return;
    const body = text.trim();
    setSending(true);
    try {
      await api.sendMessage(token, {
        apartmentId: context.apartmentId,
        text: body,
      });
      setText('');
      await refresh();
      setTimeout(() => scrollRef.current?.scrollToEnd({ animated: true }), 100);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ошибка');
    } finally {
      setSending(false);
    }
  }

  // палитра по userId — стабильный цвет для аватарки соседа
  function colorFor(userId: string): string {
    let h = 0;
    for (let i = 0; i < userId.length; i++) h = (h * 31 + userId.charCodeAt(i)) % 1_000_000;
    return ROOMMATE_PALETTE[h % ROOMMATE_PALETTE.length];
  }

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      keyboardVerticalOffset={Platform.OS === 'ios' ? 90 : 0}
      style={{ flex: 1 }}
    >
      <ScrollView
        ref={scrollRef}
        contentContainerStyle={{ padding: 16, paddingBottom: 16, gap: 8 }}
        onContentSizeChange={() => scrollRef.current?.scrollToEnd({ animated: false })}
      >
        {loading && messages.length === 0 && (
          <Glass padding={20} radius={20}>
            <Text style={[TYPE.body, { color: T.text2, textAlign: 'center' }]}>
              Загружаем…
            </Text>
          </Glass>
        )}
        {!loading && messages.length === 0 && (
          <Glass padding={20} radius={20}>
            <Text style={[TYPE.body, { color: T.text2, textAlign: 'center' }]}>
              Сообщений пока нет. Напишите первым.
            </Text>
          </Glass>
        )}
        {messages.map((m, idx) => {
          const isMe = m.senderId === context.meUserId;
          const showName =
            !isMe && (idx === 0 || messages[idx - 1].senderId !== m.senderId);
          return (
            <Bubble
              key={m.id}
              text={m.text}
              when={new Date(m.sentAt)}
              isMe={isMe}
              senderName={showName ? m.senderName : null}
              color={colorFor(m.senderId)}
            />
          );
        })}
        {error && (
          <Text style={[TYPE.footnote, { color: T.danger, marginTop: 8 }]}>{error}</Text>
        )}
      </ScrollView>

      <View
        style={{
          paddingHorizontal: 12,
          paddingTop: 8,
          paddingBottom: 96,
          flexDirection: 'row',
          gap: 8,
          alignItems: 'flex-end',
        }}
      >
        <View
          style={{
            flex: 1,
            backgroundColor: '#fff',
            borderRadius: 22,
            borderColor: T.border,
            borderWidth: 1,
            paddingHorizontal: 14,
            paddingVertical: 10,
            minHeight: 40,
            maxHeight: 120,
          }}
        >
          <TextInput
            value={text}
            onChangeText={setText}
            placeholder="Сообщение…"
            placeholderTextColor={T.text3}
            multiline
            style={{ fontSize: 15, color: T.text, padding: 0, minHeight: 20 }}
          />
        </View>
        <Pressable
          onPress={send}
          disabled={!text.trim() || sending}
          style={{
            width: 44,
            height: 44,
            borderRadius: 22,
            backgroundColor: !text.trim() || sending ? T.text3 : T.accent,
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          <Icon name="send" size={20} color="#fff" />
        </Pressable>
      </View>
    </KeyboardAvoidingView>
  );
}

function Bubble({
  text,
  when,
  isMe,
  senderName,
  color,
}: {
  text: string;
  when: Date;
  isMe: boolean;
  senderName: string | null;
  color: string;
}) {
  const time = when.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
  if (isMe) {
    return (
      <View style={{ alignSelf: 'flex-end', maxWidth: '78%' }}>
        <View
          style={{
            backgroundColor: T.accent,
            paddingHorizontal: 12,
            paddingVertical: 8,
            borderRadius: 18,
            borderBottomRightRadius: 6,
          }}
        >
          <Text style={[TYPE.body, { color: '#fff' }]}>{text}</Text>
        </View>
        <Text
          style={[TYPE.caption, { color: T.text3, alignSelf: 'flex-end', marginTop: 2 }]}
        >
          {time}
        </Text>
      </View>
    );
  }
  return (
    <View style={{ flexDirection: 'row', gap: 8, alignItems: 'flex-end', maxWidth: '85%' }}>
      <Avatar initial={(senderName ?? '?').charAt(0)} size={24} color={color} />
      <View>
        {senderName && (
          <Text style={[TYPE.caption, { color: T.text2, marginBottom: 2, paddingLeft: 4 }]}>
            {senderName}
          </Text>
        )}
        <View
          style={{
            backgroundColor: '#fff',
            paddingHorizontal: 12,
            paddingVertical: 8,
            borderRadius: 18,
            borderBottomLeftRadius: 6,
            borderColor: T.border,
            borderWidth: 0.5,
          }}
        >
          <Text style={[TYPE.body, { color: T.text }]}>{text}</Text>
        </View>
        <Text style={[TYPE.caption, { color: T.text3, marginTop: 2, paddingLeft: 4 }]}>
          {time}
        </Text>
      </View>
    </View>
  );
}
