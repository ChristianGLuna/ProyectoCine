
USE SarmiMovieDB;

-- Tabla: usuarios
CREATE TABLE usuarios (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100),
    apellido VARCHAR(100),
    email VARCHAR(150) UNIQUE,
    telefono VARCHAR(20) UNIQUE,
    contrasena_hash TEXT,
    rol ENUM('Cliente', 'Admin') NOT NULL,
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Tabla: refresh_tokens
CREATE TABLE refresh_tokens (
    id INT PRIMARY KEY AUTO_INCREMENT,
    id_usuario INT,
    token TEXT,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_expiracion DATETIME,
    valido BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id)
);

-- Tabla: sucursales
CREATE TABLE sucursales (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100),
    ubicacion VARCHAR(200)
);

-- Tabla: peliculas
CREATE TABLE peliculas (
    id INT PRIMARY KEY AUTO_INCREMENT,
    titulo VARCHAR(200),
    descripcion TEXT,
    genero VARCHAR(100),
    duracion INT,
    clasificacion VARCHAR(10),
    idioma VARCHAR(50),
    sinopsis TEXT,
    imagen VARCHAR(255),
    activa BOOLEAN DEFAULT TRUE
);

-- Tabla: salas
CREATE TABLE salas (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100),
    tipo VARCHAR(50),
    capacidad INT,
    id_sucursal INT,
    FOREIGN KEY (id_sucursal) REFERENCES sucursales(id)
);

-- Tabla: asientos
CREATE TABLE asientos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    sala_id INT,
    fila VARCHAR(5),
    numero INT,
    FOREIGN KEY (sala_id) REFERENCES salas(id)
);

-- Tabla: funciones
CREATE TABLE funciones (
    id INT PRIMARY KEY AUTO_INCREMENT,
    pelicula_id INT,
    sala_id INT,
    sucursal_id INT,
    fecha DATE,
    hora_inicio TIME,
    hora_fin TIME,
    precio DECIMAL(10,2),
    idioma VARCHAR(50),
    formato VARCHAR(20),
    estado VARCHAR(20),
    FOREIGN KEY (pelicula_id) REFERENCES peliculas(id),
    FOREIGN KEY (sala_id) REFERENCES salas(id),
    FOREIGN KEY (sucursal_id) REFERENCES sucursales(id)
);

-- Tabla: tipos_boleto
CREATE TABLE tipos_boleto (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(50)
);

-- Tabla: precios_funcion
CREATE TABLE precios_funcion (
    funcion_id INT,
    tipo_boleto_id INT,
    precio DECIMAL(6,2),
    PRIMARY KEY (funcion_id, tipo_boleto_id),
    FOREIGN KEY (funcion_id) REFERENCES funciones(id),
    FOREIGN KEY (tipo_boleto_id) REFERENCES tipos_boleto(id)
);

-- Tabla: asientos_funcion
CREATE TABLE asientos_funcion (
    funcion_id INT,
    asiento VARCHAR(10),
    disponible BOOLEAN DEFAULT TRUE,
    PRIMARY KEY (funcion_id, asiento),
    FOREIGN KEY (funcion_id) REFERENCES funciones(id)
);

-- Tabla: reservas
CREATE TABLE reservas (
    id INT PRIMARY KEY AUTO_INCREMENT,
    usuario_id INT,
    funcion_id INT,
    nombre_cliente VARCHAR(100),
    apellido_cliente VARCHAR(100),
    correo_cliente VARCHAR(150),
    fecha_reserva DATETIME DEFAULT CURRENT_TIMESTAMP,
    total DECIMAL(6,2),
    estado VARCHAR(50),
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
    FOREIGN KEY (funcion_id) REFERENCES funciones(id)
);

-- Tabla: entradas
CREATE TABLE entradas (
    id INT PRIMARY KEY AUTO_INCREMENT,
    reserva_id INT,
    tipo_boleto_id INT,
    asiento VARCHAR(10),
    precio_unitario DECIMAL(6,2),
    FOREIGN KEY (reserva_id) REFERENCES reservas(id),
    FOREIGN KEY (tipo_boleto_id) REFERENCES tipos_boleto(id)
);

-- Tabla: metodos_pago
CREATE TABLE metodos_pago (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(50)
);

-- Tabla: pagos
CREATE TABLE pagos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    reserva_id INT,
    metodo_pago_id INT,
    detalle VARCHAR(150),
    fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
    monto DECIMAL(10,2),
    referencia VARCHAR(100),
    FOREIGN KEY (reserva_id) REFERENCES reservas(id),
    FOREIGN KEY (metodo_pago_id) REFERENCES metodos_pago(id)
);

-- Tabla: mensajes_contacto
CREATE TABLE mensajes_contacto (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100),
    correo VARCHAR(150),
    mensaje TEXT,
    fecha DATETIME DEFAULT CURRENT_TIMESTAMP
);
