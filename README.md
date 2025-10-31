# Mongolian Phonetic Keyboard - Windows

Windows дээр ажиллах Phonetic Mongolian Keyboard layout.

## Юу хийдэг вэ?

Латин үсгээр бичиж, автоматаар кирилл үсэг рүү хөрвүүлдэг. Жишээ нь:

```
sain baina uu  →  сайн байна уу
mongol         →  монгол
bayarlalaa     →  баярлалаа
```

## Суулгах

1. [Releases](../../releases) хэсгээс `MongolianPhoneticKeyboard-Setup-1.0.0.exe` татаж авна
2. Installer-г ажиллуулна
3. Суулгана

## Хэрэглэх

Суулгасны дараа **М** үсэг system tray дээр гарч ирнэ (цагийн дэргэд). Default-оор асаалттай байна.

- **Асаах/унтраах**: М icon дээр хоёр товш. Эсвэл **Ctrl + Shift + M** товчнуудыг зэрэг дарж идэвхижүүлж/унтраана.
- **System tray menu**: М icon дээр баруун товш
  - Start with Windows - Компьютер асахад автоматаар эхлүүлэх
  - Exit - Хаах

Дараа нь Notepad, browser гэх мэт хаана ч бичиж болно.
Одоогоор MS Word дээр хараахан ажиллахгүй. 
## Үсгийн харгалзаа

### Энгийн үсэг

```
a→а  b→б  v→в  g→г  d→д  e→э  j→ж  z→з  i→и  k→к
l→л  m→м  n→н  o→о  p→п  r→р  s→с  t→т  u→у  f→ф
h→х  x→х  c→ц  y→ы  q→ө  w→ү  '→ь  "→ъ
```

### Хослол үсэг

```
ye→е   yo→ё   ts→ц   ch→ч   sh→ш
yu→ю   ya→я   ai→ай  ei→эй  oi→ой
ui→уй  qi→өй  wi→үй  ii→ий
```

## Асуудал гарвал

- System tray дээр М icon байгаа эсэхийг шалгана
- Icon дээр хоёр товшоод асаалттай эсэхийг шалгана
- Програм ажиллахгүй байвал administrator эрхээр ажиллуулна

## Build хийх (хөгжүүлэгчдэд)

**.NET 8.0 SDK** шаардлагатай.

```powershell
.\build.ps1
```

---

**Анхаар:** Энэ програм keyboard hook ашигладаг тул зарим antivirus software анхааруулга өгч магадгүй.
